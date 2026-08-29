using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReportMatches.GetMine;

public sealed record GetMyPetReportMatchesQuery(
    int Page = 1,
    int PageSize = 12,
    MatchStatus? Status = null)
    : IQuery<Result<
        PagedResult<MyPetReportMatchResponse>>>;