using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Persistence.Identity;

namespace RescueLink.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken =>
            refreshToken.Id);

        builder.Ignore(refreshToken =>
            refreshToken.IsActive);

        builder.Property(refreshToken =>
            refreshToken.RowVersion)
            .IsRowVersion();

        builder.Property(refreshToken =>
                refreshToken.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(refreshToken =>
                refreshToken.ReplacedByTokenHash)
            .HasMaxLength(64);

        builder.Property(refreshToken =>
                refreshToken.ExpiresAt)
            .IsRequired();

        builder.Property(refreshToken =>
                refreshToken.CreatedAt)
            .IsRequired();

        builder.HasIndex(refreshToken =>
                refreshToken.TokenHash)
            .IsUnique();

        builder.HasIndex(refreshToken => new
        {
            refreshToken.UserId,
            refreshToken.ExpiresAt
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(refreshToken =>
                refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}