using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Persistence.Identity;

namespace RescueLink.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.CountryCode)
            .HasMaxLength(2);

        builder.Property(user => user.City)
            .HasMaxLength(100);

        builder.Property(user => user.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue("en")
            .IsRequired();

        builder.Property(user => user.TimeZoneId)
            .HasMaxLength(100)
            .HasDefaultValue("UTC")
            .IsRequired();
    }
}