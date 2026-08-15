using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Domain.Entities;

namespace RescueLink.Persistence.Configurations;

internal sealed class PetReportMatchConfiguration
    : IEntityTypeConfiguration<PetReportMatch>
{
    public void Configure(
        EntityTypeBuilder<PetReportMatch> builder)
    {
        builder.ToTable("PetReportMatches");

        builder.HasKey(match => match.Id);

        builder.Property(match => match.Score)
            .IsRequired();

        builder.Property(match => match.DistanceMeters)
            .IsRequired();

        builder.Property(match => match.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(match => new
        {
            match.LostReportId,
            match.FoundReportId
        })
        .IsUnique();

        builder.HasIndex(match => new
        {
            match.Status,
            match.Score
        });

        builder.HasOne<PetReport>()
            .WithMany()
            .HasForeignKey(match => match.LostReportId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<PetReport>()
            .WithMany()
            .HasForeignKey(match => match.FoundReportId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_PetReportMatches_Score",
                "[Score] >= 0 AND [Score] <= 100");

            tableBuilder.HasCheckConstraint(
                "CK_PetReportMatches_DistanceMeters",
                "[DistanceMeters] >= 0");
        });
    }
}