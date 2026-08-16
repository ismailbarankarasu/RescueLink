using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports.GetList;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Abstractions.Data;

public interface IPetReportListReadService
{
    Task<PagedResult<PetReportListItemResponse>> GetListAsync(
        int page,
        int pageSize,
        ReportType? reportType,
        AnimalSpecies? species,
        string? search,
        CancellationToken cancellationToken = default);
}