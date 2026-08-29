using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReportMatches.GetMine;

public sealed class GetMyPetReportMatchesQueryHandler
    : IRequestHandler<
        GetMyPetReportMatchesQuery,
        Result<PagedResult<MyPetReportMatchResponse>>>
{
    private readonly IMyPetReportMatchReadService
        _readService;

    private readonly ICurrentUserService
        _currentUserService;

    public GetMyPetReportMatchesQueryHandler(
        IMyPetReportMatchReadService readService,
        ICurrentUserService currentUserService)
    {
        _readService = readService;
        _currentUserService = currentUserService;
    }

    public async Task<
        Result<PagedResult<MyPetReportMatchResponse>>> Handle(
            GetMyPetReportMatchesQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                PagedResult<MyPetReportMatchResponse>>(
                    PetReportMatchErrors.Unauthenticated);
        }

        var result = await _readService.GetAsync(
            userId: _currentUserService.UserId.Value,
            page: request.Page,
            pageSize: request.PageSize,
            status: request.Status,
            cancellationToken: cancellationToken);

        return Result.Success(result);
    }
}