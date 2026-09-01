using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Domain.Common;
using RescueLink.Persistence.Context;

namespace RescueLink.Persistence.Outbox;

internal sealed class OutboxMessageProcessor(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<OutboxMessageProcessor> logger)
    : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaximumRetryCount = 10;

    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(5);

    private static readonly TimeSpan LockDuration =
        TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(
                    stoppingToken);

                await Task.Delay(
                    PollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken
                    .IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "An error occurred while processing " +
                    "outbox messages.");

                await DelayAfterFailureAsync(
                    stoppingToken);
            }
        }
    }

    private async Task ProcessMessagesAsync(
        CancellationToken cancellationToken)
    {
        var claimedMessages =
            await ClaimMessagesAsync(
                cancellationToken);

        foreach (var claimedMessage in
                 claimedMessages)
        {
            if (cancellationToken
                .IsCancellationRequested)
            {
                break;
            }

            await ProcessMessageAsync(
                claimedMessage.MessageId,
                claimedMessage.LockId,
                cancellationToken);
        }
    }

    private async Task<IReadOnlyCollection<
        ClaimedOutboxMessage>> ClaimMessagesAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    RescueLinkDbContext>();

        var now =
            timeProvider.GetUtcNow();

        var lockId =
            Guid.NewGuid();

        var lockedUntilUtc =
            now.Add(LockDuration);

        await using var transaction =
            await dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        const string sql = """
            ;WITH ClaimableMessages AS
            (
                SELECT TOP (@BatchSize)
                    message.*
                FROM dbo.OutboxMessages AS message
                    WITH
                    (
                        UPDLOCK,
                        READPAST,
                        ROWLOCK
                    )
                WHERE message.ProcessedOnUtc IS NULL
                  AND message.RetryCount <
                        @MaximumRetryCount
                  AND message.NextAttemptOnUtc <= @Now
                  AND
                  (
                      message.LockedUntilUtc IS NULL
                      OR message.LockedUntilUtc <= @Now
                  )
                ORDER BY message.OccurredOnUtc
            )
            UPDATE ClaimableMessages
            SET
                LockId = @LockId,
                LockedUntilUtc = @LockedUntilUtc;
            """;

        await dbContext.Database
            .ExecuteSqlRawAsync(
                sql,
                [
                    new Microsoft.Data.SqlClient
                        .SqlParameter(
                            "@BatchSize",
                            BatchSize),

                    new Microsoft.Data.SqlClient
                        .SqlParameter(
                            "@MaximumRetryCount",
                            MaximumRetryCount),

                    new Microsoft.Data.SqlClient
                        .SqlParameter(
                            "@Now",
                            now),

                    new Microsoft.Data.SqlClient
                        .SqlParameter(
                            "@LockId",
                            lockId),

                    new Microsoft.Data.SqlClient
                        .SqlParameter(
                            "@LockedUntilUtc",
                            lockedUntilUtc)
                ],
                cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        var messageIds =
            await dbContext.OutboxMessages
                .AsNoTracking()
                .Where(message =>
                    message.LockId == lockId)
                .OrderBy(message =>
                    message.OccurredOnUtc)
                .Select(message => message.Id)
                .ToArrayAsync(cancellationToken);

        return messageIds
            .Select(messageId =>
                new ClaimedOutboxMessage(
                    MessageId: messageId,
                    LockId: lockId))
            .ToArray();
    }

    private async Task ProcessMessageAsync(
        Guid messageId,
        Guid lockId,
        CancellationToken cancellationToken)
    {
        try
        {
            await ProcessClaimedMessageAsync(
                messageId,
                lockId,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken
                .IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await MarkMessageAsFailedAsync(
                messageId,
                lockId,
                exception,
                cancellationToken);

            logger.LogError(
                exception,
                "Outbox message {OutboxMessageId} " +
                "could not be processed.",
                messageId);
        }
    }

    private async Task ProcessClaimedMessageAsync(
        Guid messageId,
        Guid lockId,
        CancellationToken cancellationToken)
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    RescueLinkDbContext>();

        var dispatcher =
            scope.ServiceProvider
                .GetRequiredService<
                    IDomainEventDispatcher>();

        await using var transaction =
            await dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        var message =
            await dbContext.OutboxMessages
                .SingleOrDefaultAsync(
                    outboxMessage =>
                        outboxMessage.Id ==
                            messageId &&
                        outboxMessage.LockId ==
                            lockId &&
                        outboxMessage
                            .ProcessedOnUtc == null,
                    cancellationToken);

        if (message is null)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            return;
        }

        try
        {
            var eventType =
                Type.GetType(
                    message.Type,
                    throwOnError: false)
                ?? throw new InvalidOperationException(
                    $"Domain event type " +
                    $"'{message.Type}' could not " +
                    "be found.");

            var domainEvent =
                JsonSerializer.Deserialize(
                    message.Content,
                    eventType)
                as IDomainEvent
                ?? throw new InvalidOperationException(
                    "Outbox message could not be " +
                    "deserialized as a domain event.");

            await dispatcher.DispatchAsync(
                [domainEvent],
                cancellationToken);

            message.MarkAsProcessed(
                timeProvider.GetUtcNow());

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task MarkMessageAsFailedAsync(
        Guid messageId,
        Guid lockId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    RescueLinkDbContext>();

        var message =
            await dbContext.OutboxMessages
                .SingleOrDefaultAsync(
                    outboxMessage =>
                        outboxMessage.Id ==
                            messageId &&
                        outboxMessage.LockId ==
                            lockId &&
                        outboxMessage
                            .ProcessedOnUtc == null,
                    cancellationToken);

        if (message is null)
        {
            return;
        }

        var retryDelay =
            TimeSpan.FromMinutes(
                Math.Min(
                    Math.Pow(
                        2,
                        message.RetryCount),
                    60));

        message.MarkAsFailed(
            exception.ToString(),
            timeProvider
                .GetUtcNow()
                .Add(retryDelay));

        await dbContext.SaveChangesAsync(
            cancellationToken);

        logger.LogWarning(
            "Outbox message {OutboxMessageId} " +
            "was scheduled for retry. " +
            "Retry count: {RetryCount}.",
            message.Id,
            message.RetryCount);
    }

    private static async Task DelayAfterFailureAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(
                PollingInterval,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken
                .IsCancellationRequested)
        {
            // Application is stopping.
        }
    }

    private sealed record ClaimedOutboxMessage(
        Guid MessageId,
        Guid LockId);
}