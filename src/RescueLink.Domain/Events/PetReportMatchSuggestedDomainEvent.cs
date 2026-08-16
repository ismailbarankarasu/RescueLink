using RescueLink.Domain.Common;

namespace RescueLink.Domain.Events;

public sealed record PetReportMatchSuggestedDomainEvent(
    Guid MatchId,
    Guid LostReportId,
    Guid FoundReportId)
    : DomainEvent;