using RescueLink.Domain.Common;

namespace RescueLink.Domain.Events;

public sealed record PetReportCreatedDomainEvent(
    Guid PetReportId)
    : DomainEvent;