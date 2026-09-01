using System.Collections.Concurrent;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RescueLink.API.IntegrationTests.Infrastructure;
using RescueLink.Application.Common.Events;
using RescueLink.Domain.Common;
using RescueLink.Persistence.Context;
using RescueLink.Persistence.Outbox;

namespace RescueLink.API.IntegrationTests.Features.Outbox;

public sealed class OutboxMessageProcessorTests : IClassFixture<SqlServerContainerFixture>
{
    private readonly SqlServerContainerFixture _sqlServerContainer;

    public OutboxMessageProcessorTests(SqlServerContainerFixture sqlServerContainer)
    {
        _sqlServerContainer = sqlServerContainer;
    }

    [Fact]
    public async Task MultipleInstances_ShouldProcessMessageOnlyOnce()
    {
        // Arrange
        OutboxLeaseTestTracker.Reset();

        await using var firstBaseFactory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        await using var secondBaseFactory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        await using var firstFactory =
            firstBaseFactory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureServices(
                        services =>
                        {
                            services.AddTransient<
                                INotificationHandler<
                                    DomainEventNotification<
                                        OutboxLeaseTestDomainEvent>>,
                                OutboxLeaseTestDomainEventHandler>();
                        });
                });

        await using var secondFactory =
            secondBaseFactory.WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureServices(
                        services =>
                        {
                            services.AddTransient<
                                INotificationHandler<
                                    DomainEventNotification<
                                        OutboxLeaseTestDomainEvent>>,
                                OutboxLeaseTestDomainEventHandler>();
                        });
                });

        // CreateClient iki uygulamayı da başlatır.
        using var firstClient =
            firstFactory.CreateClient();

        using var secondClient =
            secondFactory.CreateClient();

        var testId =
            Guid.NewGuid();

        var domainEvent =
            new OutboxLeaseTestDomainEvent(
                testId);

        var eventType =
            domainEvent.GetType();

        var typeName =
            eventType.AssemblyQualifiedName
            ?? throw new InvalidOperationException(
                "Test domain event type name " +
                "could not be determined.");

        var content =
            JsonSerializer.Serialize(
                domainEvent,
                eventType);

        Guid outboxMessageId;

        await using (
            var scope =
                firstFactory.Services
                    .CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        RescueLinkDbContext>();

            var message =
                OutboxMessage.Create(
                    occurredOnUtc:
                        domainEvent.OccurredAt,
                    type: typeName,
                    content: content);

            outboxMessageId =
                message.Id;

            await dbContext.OutboxMessages
                .AddAsync(message);

            await dbContext.SaveChangesAsync();
        }

        // Act
        await WaitForMessageToBeProcessedAsync(
            firstFactory.Services,
            outboxMessageId);

        // İkinci worker'ın da yeni polling turuna
        // girmesi için yeterli süre tanıyoruz.
        await Task.Delay(
            TimeSpan.FromSeconds(6));

        // Assert
        OutboxLeaseTestTracker
            .GetExecutionCount(testId)
            .Should()
            .Be(
                1,
                "an outbox message must be claimed " +
                "by only one application instance");

        await using var assertionScope =
            firstFactory.Services
                .CreateAsyncScope();

        var assertionDbContext =
            assertionScope.ServiceProvider
                .GetRequiredService<
                    RescueLinkDbContext>();

        var persistedMessage =
            await assertionDbContext
                .OutboxMessages
                .AsNoTracking()
                .SingleAsync(message =>
                    message.Id ==
                    outboxMessageId);

        persistedMessage.ProcessedOnUtc
            .Should()
            .NotBeNull();

        persistedMessage.LockId
            .Should()
            .BeNull();

        persistedMessage.LockedUntilUtc
            .Should()
            .BeNull();

        persistedMessage.RetryCount
            .Should()
            .Be(0);

        persistedMessage.Error
            .Should()
            .BeNull();

        OutboxLeaseTestTracker.Remove(
            testId);
    }

    private static async Task WaitForMessageToBeProcessedAsync(IServiceProvider serviceProvider, Guid messageId)
    {
        var timeoutAt =
            DateTimeOffset.UtcNow
                .AddSeconds(20);

        while (DateTimeOffset.UtcNow <
               timeoutAt)
        {
            await using var scope =
                serviceProvider
                    .CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        RescueLinkDbContext>();

            var isProcessed =
                await dbContext.OutboxMessages
                    .AsNoTracking()
                    .Where(message =>
                        message.Id == messageId)
                    .Select(message =>
                        message.ProcessedOnUtc != null)
                    .SingleAsync();

            if (isProcessed)
            {
                return;
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(250));
        }

        throw new TimeoutException(
            "The outbox message was not processed " +
            "within 20 seconds.");
    }
}

public sealed record OutboxLeaseTestDomainEvent(Guid TestId) : IDomainEvent
{
    public DateTimeOffset OccurredAt
    {
        get;
        init;
    } = DateTimeOffset.UtcNow;
}

public sealed class
    OutboxLeaseTestDomainEventHandler
    : INotificationHandler<
        DomainEventNotification<
            OutboxLeaseTestDomainEvent>>
{
    public Task Handle(
        DomainEventNotification<
            OutboxLeaseTestDomainEvent>
            notification,
        CancellationToken cancellationToken)
    {
        OutboxLeaseTestTracker.Record(
            notification.DomainEvent.TestId);

        return Task.CompletedTask;
    }
}

internal static class OutboxLeaseTestTracker
{
    private static readonly
        ConcurrentDictionary<Guid, int>
        ExecutionCounts = new();

    public static void Record(
        Guid testId)
    {
        ExecutionCounts.AddOrUpdate(
            testId,
            addValue: 1,
            updateValueFactory:
                (_, currentCount) =>
                    currentCount + 1);
    }

    public static int GetExecutionCount(
        Guid testId)
    {
        return ExecutionCounts.TryGetValue(
            testId,
            out var executionCount)
                ? executionCount
                : 0;
    }

    public static void Remove(
        Guid testId)
    {
        ExecutionCounts.TryRemove(
            testId,
            out _);
    }

    public static void Reset()
    {
        ExecutionCounts.Clear();
    }
}