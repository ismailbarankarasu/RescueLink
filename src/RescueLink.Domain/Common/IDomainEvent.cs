namespace RescueLink.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}