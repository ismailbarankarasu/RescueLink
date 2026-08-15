using RescueLink.Domain.Common;

namespace RescueLink.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}