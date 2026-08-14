using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Persistence;
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