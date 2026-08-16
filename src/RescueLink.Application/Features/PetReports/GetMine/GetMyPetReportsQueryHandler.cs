using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.GetMine;

public sealed class GetMyPetReportsQueryHandler
    : IRequestHandler<
        GetMyPetReportsQuery,
        Result<PagedResult<MyPetReportListItemResponse>>>
{
    private readonly IMyPetReportReadService
        _myPetReportReadService;

    private readonly ICurrentUserService
        _currentUserService;

    public GetMyPetReportsQueryHandler(
        IMyPetReportReadService myPetReportReadService,
        ICurrentUserService currentUserService)
    {
        _myPetReportReadService = myPetReportReadService;
        _currentUserService = currentUserService;
    }

    public async Task<
        Result<PagedResult<MyPetReportListItemResponse>>> Handle(
            GetMyPetReportsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                PagedResult<MyPetReportListItemResponse>>(
                    PetReportErrors.Unauthenticated);
        }

        var result = await _myPetReportReadService.GetAsync(
            userId: _currentUserService.UserId.Value,
            page: request.Page,
            pageSize: request.PageSize,
            reportType: request.ReportType,
            status: request.Status,
            cancellationToken: cancellationToken);

        return Result.Success(result);
    }
}