using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports
    .Matching.Recalculate;

public sealed record RecalculatePetReportMatchesCommand(
    Guid PetReportId)
    : ICommand<Result>;