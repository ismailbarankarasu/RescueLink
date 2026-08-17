using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Common;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Identity;

namespace RescueLink.Persistence.Context;

public sealed class RescueLinkDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>,
      IUnitOfWork
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public RescueLinkDbContext(
        DbContextOptions<RescueLinkDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
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

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entitiesWithDomainEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();

        var result = await base.SaveChangesAsync(
            cancellationToken);

        foreach (var entity in entitiesWithDomainEvents)
        {
            entity.ClearDomainEvents();
        }

        if (domainEvents.Length > 0)
        {
            await _domainEventDispatcher.DispatchAsync(
                domainEvents,
                cancellationToken);
        }

        return result;
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RescueLinkDbContext).Assembly);
    }
}