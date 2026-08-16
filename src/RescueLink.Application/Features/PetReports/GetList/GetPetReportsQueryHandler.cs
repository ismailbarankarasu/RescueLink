using MediatR;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.GetList;

public sealed class GetPetReportsQueryHandler
    : IRequestHandler<
        GetPetReportsQuery,
        Result<PagedResult<PetReportListItemResponse>>>
{
    private readonly IPetReportListReadService _readService;

    public GetPetReportsQueryHandler(
        IPetReportListReadService readService)
    {
        _readService = readService;
    }

    public async Task<
        Result<PagedResult<PetReportListItemResponse>>>
        Handle(
            GetPetReportsQuery request,
            CancellationToken cancellationToken)
    {
        var pagedResult = await _readService.GetListAsync(
            page: request.Page,
            pageSize: request.PageSize,
            reportType: request.ReportType,
            species: request.Species,
            search: request.Search,
            cancellationToken: cancellationToken);

        return Result.Success(pagedResult);
    }
}