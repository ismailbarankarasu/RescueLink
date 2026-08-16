using RescueLink.Domain.Common;

namespace RescueLink.Domain.Events;

public sealed record PetReportMatchConfirmedDomainEvent(
    Guid MatchId,
    Guid LostReportId,
    Guid FoundReportId)
    : DomainEvent;