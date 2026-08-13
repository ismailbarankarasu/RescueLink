using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Domain.Entities;

namespace RescueLink.Persistence.Configurations;

public sealed class PetReportPhotoConfiguration
    : IEntityTypeConfiguration<PetReportPhoto>
{
    public void Configure(
        EntityTypeBuilder<PetReportPhoto> builder)
    {
        builder.ToTable("PetReportPhotos");

        builder.HasKey(photo => photo.Id);

        builder.Property(photo => photo.Id)
            .ValueGeneratedNever();

        builder.Property(photo => photo.PetReportId)
            .IsRequired();

        builder.Property(photo => photo.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(photo => photo.IsPrimary)
            .IsRequired();

        builder.Property(photo => photo.DisplayOrder)
            .IsRequired();

        builder.Property(photo => photo.CreatedAt)
            .IsRequired();

        builder.Property(photo => photo.UpdatedAt);

        builder.HasIndex(photo => new
        {
            photo.PetReportId,
            photo.StorageKey
        })
            .IsUnique();

        builder.HasIndex(photo => new
        {
            photo.PetReportId,
            photo.DisplayOrder
        })
            .IsUnique();
    }
}