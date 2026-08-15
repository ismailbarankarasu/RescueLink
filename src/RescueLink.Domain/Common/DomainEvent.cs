namespace RescueLink.Domain.Common;

public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } =
        DateTimeOffset.UtcNow;
}