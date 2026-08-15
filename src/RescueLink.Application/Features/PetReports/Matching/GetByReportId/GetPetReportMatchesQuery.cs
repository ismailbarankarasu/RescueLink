using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;

public sealed record GetPetReportMatchesQuery(
    Guid PetReportId)
    : IQuery<Result<
        IReadOnlyCollection<PetReportMatchResponse>>>;