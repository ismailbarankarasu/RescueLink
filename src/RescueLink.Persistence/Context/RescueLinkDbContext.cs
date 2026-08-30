using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Common;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Identity;
using RescueLink.Persistence.Outbox;
using System.Data;
using System.Text.Json;

namespace RescueLink.Persistence.Context;

public sealed class RescueLinkDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IUnitOfWork
{

    public RescueLinkDbContext(DbContextOptions<RescueLinkDbContext> options) : base(options)
    {
    }
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<PetReport> PetReports => Set<PetReport>();

    public DbSet<PetReportPhoto> PetReportPhotos => Set<PetReportPhoto>();
    public DbSet<PetReportMatch> PetReportMatches => Set<PetReportMatch>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithDomainEvents =
            ChangeTracker
                .Entries<BaseEntity>()
                .Select(entry => entry.Entity)
                .Where(entity =>
                    entity.DomainEvents.Count > 0)
                .ToArray();

        var domainEvents =
            entitiesWithDomainEvents
                .SelectMany(entity =>
                    entity.DomainEvents)
                .ToArray();

        var outboxMessages =
            domainEvents
                .Select(domainEvent =>
                {
                    var eventType =
                        domainEvent.GetType();

                    var typeName =
                        eventType.AssemblyQualifiedName
                        ?? throw new InvalidOperationException(
                            "Domain event type name " +
                            "could not be determined.");

                    var content =
                        JsonSerializer.Serialize(
                            domainEvent,
                            eventType);

                    return OutboxMessage.Create(
                        occurredOnUtc:
                            DateTimeOffset.UtcNow,
                        type: typeName,
                        content: content);
                })
                .ToArray();

        if (outboxMessages.Length > 0)
        {
            await OutboxMessages.AddRangeAsync(
                outboxMessages,
                cancellationToken);
        }

        try
        {
            var result =
                await base.SaveChangesAsync(
                    cancellationToken);

            foreach (var entity in
                     entitiesWithDomainEvents)
            {
                entity.ClearDomainEvents();
            }

            return result;
        }
        catch
        {
            foreach (var outboxMessage in
                     outboxMessages)
            {
                Entry(outboxMessage).State =
                    EntityState.Detached;
            }

            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RescueLinkDbContext).Assembly);
    }

    public async Task AcquireTransactionLockAsync(string resource, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        var currentTransaction =
            Database.CurrentTransaction
            ?? throw new InvalidOperationException(
                "An active transaction is required " +
                "before acquiring an application lock.");

        var connection =
            Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        await using var command =
            connection.CreateCommand();

        command.Transaction =
            currentTransaction.GetDbTransaction();

        command.CommandText = """
        DECLARE @Result int;

        EXEC @Result = sys.sp_getapplock
            @Resource = @Resource,
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 10000;

        SELECT @Result;
        """;

        var resourceParameter =
            command.CreateParameter();

        resourceParameter.ParameterName =
            "@Resource";

        resourceParameter.DbType =
            DbType.String;

        resourceParameter.Value =
            resource;

        command.Parameters.Add(
            resourceParameter);

        var result =
            await command.ExecuteScalarAsync(
                cancellationToken);

        var lockResult =
            Convert.ToInt32(result);

        if (lockResult < 0)
        {
            throw new TimeoutException(
                $"Application lock could not be " +
                $"acquired for resource '{resource}'. " +
                $"Result code: {lockResult}.");
        }
    }
}