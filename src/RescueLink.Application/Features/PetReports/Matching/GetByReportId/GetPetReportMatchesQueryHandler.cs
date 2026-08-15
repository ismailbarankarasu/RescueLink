using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;

public sealed class GetPetReportMatchesQueryHandler
    : IRequestHandler<
        GetPetReportMatchesQuery,
        Result<IReadOnlyCollection<PetReportMatchResponse>>>
{
    private readonly IPetReportRepository _petReportRepository;
    private readonly IPetReportMatchReadService _matchReadService;
    private readonly ICurrentUserService _currentUserService;

    public GetPetReportMatchesQueryHandler(
        IPetReportRepository petReportRepository,
        IPetReportMatchReadService matchReadService,
        ICurrentUserService currentUserService)
    {
        _petReportRepository = petReportRepository;
        _matchReadService = matchReadService;
        _currentUserService = currentUserService;
    }

    public async Task<
        Result<IReadOnlyCollection<PetReportMatchResponse>>>
        Handle(
            GetPetReportMatchesQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                IReadOnlyCollection<PetReportMatchResponse>>(
                PetReportErrors.Unauthenticated);
        }

        var petReport =
            await _petReportRepository.GetByIdReadOnlyAsync(
                request.PetReportId,
                cancellationToken);

        if (petReport is null)
        {
            return Result.Failure<
                IReadOnlyCollection<PetReportMatchResponse>>(
                PetReportErrors.NotFound(request.PetReportId));
        }

        if (petReport.UserId !=
            _currentUserService.UserId.Value)
        {
            return Result.Failure<
                IReadOnlyCollection<PetReportMatchResponse>>(
                PetReportErrors.Forbidden);
        }

        var matches =
            await _matchReadService.GetByReportIdAsync(
                request.PetReportId,
                cancellationToken);

        return Result.Success(matches);
    }
}