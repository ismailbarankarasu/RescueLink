using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.GetList;

public sealed record GetPetReportsQuery(
    int Page = 1,
    int PageSize = 12,
    ReportType? ReportType = null,
    AnimalSpecies? Species = null,
    string? Search = null)
    : IQuery<Result<
        PagedResult<PetReportListItemResponse>>>;