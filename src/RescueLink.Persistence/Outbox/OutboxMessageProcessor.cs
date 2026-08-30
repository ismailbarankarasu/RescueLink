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

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "An error occurred while processing " +
                    "outbox messages.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    RescueLinkDbContext>();

        var dispatcher =
            scope.ServiceProvider
                .GetRequiredService<
                    IDomainEventDispatcher>();

        var now = timeProvider.GetUtcNow();

        var messages =
            await dbContext.OutboxMessages
                .Where(message =>
                    message.ProcessedOnUtc == null &&
                    message.RetryCount <
                        MaximumRetryCount &&
                    message.NextAttemptOnUtc <= now)
                .OrderBy(message =>
                    message.OccurredOnUtc)
                .Take(BatchSize)
                .ToArrayAsync(cancellationToken);

        if (messages.Length == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType =
                    Type.GetType(
                        message.Type,
                        throwOnError: false)
                    ?? throw new InvalidOperationException(
                        $"Domain event type " +
                        $"'{message.Type}' could not be found.");

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
            }
            catch (Exception exception)
            {
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

                logger.LogError(
                    exception,
                    "Outbox message {OutboxMessageId} " +
                    "could not be processed. " +
                    "Retry count: {RetryCount}.",
                    message.Id,
                    message.RetryCount);
            }
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}