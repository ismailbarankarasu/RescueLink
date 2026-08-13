using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Converters;

namespace RescueLink.Persistence.Configurations;

public sealed class PetReportConfiguration
    : IEntityTypeConfiguration<PetReport>
{
    public void Configure(EntityTypeBuilder<PetReport> builder)
    {
        builder.ToTable("PetReports");

        builder.HasKey(report => report.Id);

        builder.Property(report => report.Id)
            .ValueGeneratedNever();

        builder.Property(report => report.UserId)
            .IsRequired();

        builder.Property(report => report.ReportType)
            .IsRequired();

        builder.Property(report => report.Status)
            .IsRequired();

        builder.Property(report => report.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(report => report.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(report => report.Species)
            .IsRequired();

        builder.Property(report => report.Gender)
            .IsRequired();

        builder.Property(report => report.PetName)
            .HasMaxLength(100);

        builder.Property(report => report.Breed)
            .HasMaxLength(100);

        builder.Property(report => report.PrimaryColor)
            .IsRequired();

        builder.Property(report => report.SecondaryColor);

        builder.Property(report => report.EventDate)
            .IsRequired();

        builder.Property(report => report.Location)
            .HasConversion<GeoLocationConverter>()
            .HasColumnType("geography")
            .IsRequired();

        builder.Property(report => report.CreatedAt)
            .IsRequired();

        builder.Property(report => report.UpdatedAt);

        builder.HasMany(report => report.Photos)
            .WithOne()
            .HasForeignKey(photo => photo.PetReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(report => report.Photos)
            .HasField("_photos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(report => report.UserId);

        builder.HasIndex(report => new
        {
            report.ReportType,
            report.Status,
            report.Species
        });
    }
}