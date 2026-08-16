using RescueLink.Domain.Common;

namespace RescueLink.Domain.Events;

public sealed record PetReportUpdatedDomainEvent(
    Guid PetReportId)
    : DomainEvent;