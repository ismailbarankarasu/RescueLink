using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Identity;

namespace RescueLink.Persistence.Configurations;

internal sealed class UserNotificationConfiguration
    : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(
        EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("UserNotifications");

        builder.HasKey(notification =>
            notification.Id);

        builder.Property(notification =>
                notification.UserId)
            .IsRequired();

        builder.Property(notification =>
                notification.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(notification =>
                notification.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(notification =>
                notification.Message)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(notification =>
                notification.RelatedEntityId)
            .IsRequired(false);

        builder.Property(notification =>
                notification.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(notification =>
                notification.ReadAt)
            .IsRequired(false);

        builder.HasIndex(notification => new
        {
            notification.UserId,
            notification.IsRead,
            notification.CreatedAt
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(notification =>
                notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}