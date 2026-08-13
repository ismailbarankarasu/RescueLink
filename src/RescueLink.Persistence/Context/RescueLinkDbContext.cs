using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Entities;

namespace RescueLink.Persistence.Context;

public sealed class RescueLinkDbContext
    : DbContext, IUnitOfWork
{
    public RescueLinkDbContext(
        DbContextOptions<RescueLinkDbContext> options)
        : base(options)
    {
    }

    public DbSet<PetReport> PetReports =>
        Set<PetReport>();

    public DbSet<PetReportPhoto> PetReportPhotos =>
        Set<PetReportPhoto>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RescueLinkDbContext).Assembly);
    }
}