using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueLink.Persistence.Outbox;

namespace RescueLink.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(
        EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.OccurredOnUtc)
            .IsRequired();

        builder.Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.Content)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasMaxLength(2000);

        builder.Property(message => message.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(message => message.NextAttemptOnUtc)
            .IsRequired();

        builder.HasIndex(message => new
        {
            message.ProcessedOnUtc,
            message.NextAttemptOnUtc
        });
    }
}