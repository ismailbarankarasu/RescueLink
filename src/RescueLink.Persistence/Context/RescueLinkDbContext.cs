using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Common;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Identity;
using RescueLink.Persistence.Outbox;
using System.Text.Json;

namespace RescueLink.Persistence.Context;

public sealed class RescueLinkDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>,
      IUnitOfWork
{

    public RescueLinkDbContext(
        DbContextOptions<RescueLinkDbContext> options)
        : base(options)
    {
    }
    public DbSet<UserNotification> UserNotifications =>
        Set<UserNotification>();
    public DbSet<PetReport> PetReports =>
        Set<PetReport>();

    public DbSet<PetReportPhoto> PetReportPhotos =>
        Set<PetReportPhoto>();
    public DbSet<PetReportMatch> PetReportMatches =>
        Set<PetReportMatch>();
    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();
    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RescueLinkDbContext).Assembly);
    }
}