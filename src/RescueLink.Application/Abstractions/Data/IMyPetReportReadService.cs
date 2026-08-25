using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports.GetMine;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Abstractions.Data;

public interface IMyPetReportReadService
{
    Task<PagedResult<MyPetReportListItemResponse>> GetAsync(
        Guid userId,
        int page,
        int pageSize,
        ReportType? reportType,
        ReportStatus? status,
        bool archivedOnly,
        CancellationToken cancellationToken = default);
}