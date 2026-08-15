using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReportMatches.Reject;

public sealed class RejectPetReportMatchCommandHandler
    : IRequestHandler<RejectPetReportMatchCommand, Result>
{
    private readonly IPetReportMatchRepository _matchRepository;
    private readonly IPetReportRepository _petReportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectPetReportMatchCommandHandler(
        IPetReportMatchRepository matchRepository,
        IPetReportRepository petReportRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _petReportRepository = petReportRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RejectPetReportMatchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(
                PetReportMatchErrors.Unauthenticated);
        }

        var match = await _matchRepository.GetByIdAsync(
            request.MatchId,
            cancellationToken);

        if (match is null)
        {
            return Result.Failure(
                PetReportMatchErrors.NotFound(request.MatchId));
        }

        if (match.Status != MatchStatus.Suggested)
        {
            return Result.Failure(
                PetReportMatchErrors.NotSuggested);
        }

        var reportIds = new[]
        {
            match.LostReportId,
            match.FoundReportId
        };

        var reports =
            await _petReportRepository.GetByIdsReadOnlyAsync(
                reportIds,
                cancellationToken);

        var ownedReport = reports.FirstOrDefault(
            report =>
                report.UserId ==
                _currentUserService.UserId.Value);

        if (ownedReport is null)
        {
            return Result.Failure(
                PetReportMatchErrors.Forbidden);
        }

        match.Reject(ownedReport.Id);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}