using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReportMatches.GetMine;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Abstractions.Data;

public interface IMyPetReportMatchReadService
{
    Task<PagedResult<MyPetReportMatchResponse>> GetAsync(
        Guid userId,
        int page,
        int pageSize,
        MatchStatus? status,
        CancellationToken cancellationToken = default);
}