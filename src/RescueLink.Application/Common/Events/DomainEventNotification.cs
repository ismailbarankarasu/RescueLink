using MediatR;
using RescueLink.Domain.Common;

namespace RescueLink.Application.Common.Events;

public sealed record DomainEventNotification<TDomainEvent>(
    TDomainEvent DomainEvent)
    : INotification
    where TDomainEvent : IDomainEvent;