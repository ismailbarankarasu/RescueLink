using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.GetMine;

public sealed record GetMyPetReportsQuery(
    int Page = 1,
    int PageSize = 12,
    ReportType? ReportType = null,
    ReportStatus? Status = null,
    bool ArchivedOnly = false)
    : IQuery<Result<
        PagedResult<MyPetReportListItemResponse>>>;