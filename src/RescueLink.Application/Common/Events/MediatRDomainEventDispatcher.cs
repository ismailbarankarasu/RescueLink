using MediatR;
using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Domain.Common;

namespace RescueLink.Application.Common.Events;

internal sealed class MediatRDomainEventDispatcher(
    IPublisher publisher)
    : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            var notificationType =
                typeof(DomainEventNotification<>)
                    .MakeGenericType(domainEvent.GetType());

            var notification = Activator.CreateInstance(
                notificationType,
                domainEvent) as INotification
                ?? throw new InvalidOperationException(
                    "Domain event notification could not be created.");

            await publisher.Publish(
                notification,
                cancellationToken);
        }
    }
}