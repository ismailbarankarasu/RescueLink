using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReportMatches.Reject;

public sealed record RejectPetReportMatchCommand(
    Guid MatchId)
    : ICommand<Result>;