using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReportMatches.Confirm;

public sealed record ConfirmPetReportMatchCommand(
    Guid MatchId)
    : ICommand<Result>;