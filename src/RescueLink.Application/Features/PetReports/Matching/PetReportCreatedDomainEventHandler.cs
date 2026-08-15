using MediatR;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;
using RescueLink.Domain.Services;

namespace RescueLink.Application.Features.PetReports.Matching;

public sealed class PetReportCreatedDomainEventHandler
    : INotificationHandler<
        DomainEventNotification<PetReportCreatedDomainEvent>>
{
    private const int CandidateLimit = 50;

    private readonly IPetReportRepository _petReportRepository;
    private readonly IPetReportMatchRepository _matchRepository;
    private readonly IPetReportMatchCandidateReadService
        _candidateReadService;
    private readonly IUnitOfWork _unitOfWork;

    public PetReportCreatedDomainEventHandler(
        IPetReportRepository petReportRepository,
        IPetReportMatchRepository matchRepository,
        IPetReportMatchCandidateReadService candidateReadService,
        IUnitOfWork unitOfWork)
    {
        _petReportRepository = petReportRepository;
        _matchRepository = matchRepository;
        _candidateReadService = candidateReadService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DomainEventNotification<PetReportCreatedDomainEvent>
            notification,
        CancellationToken cancellationToken)
    {
        var sourceReport =
            await _petReportRepository.GetByIdReadOnlyAsync(
                notification.DomainEvent.PetReportId,
                cancellationToken);

        if (sourceReport is null ||
            sourceReport.Status != ReportStatus.Active)
        {
            return;
        }

        var candidateReportType =
            sourceReport.ReportType == ReportType.Lost
                ? ReportType.Found
                : ReportType.Lost;

        var candidateDistances =
            await _candidateReadService.GetCandidatesAsync(
                sourceReportId: sourceReport.Id,
                sourceUserId: sourceReport.UserId,
                candidateReportType: candidateReportType,
                species: sourceReport.Species,
                latitude: sourceReport.Location.Latitude,
                longitude: sourceReport.Location.Longitude,
                maximumDistanceMeters:
                    PetReportMatchScoreCalculator
                        .MaximumDistanceMeters,
                limit: CandidateLimit,
                cancellationToken: cancellationToken);

        if (candidateDistances.Count == 0)
        {
            return;
        }

        var candidateIds = candidateDistances
            .Select(candidate => candidate.PetReportId)
            .ToArray();

        var candidateReports =
            await _petReportRepository.GetByIdsReadOnlyAsync(
                candidateIds,
                cancellationToken);

        var existingCounterpartIds =
            await _matchRepository
                .GetExistingCounterpartIdsAsync(
                    sourceReportId: sourceReport.Id,
                    sourceReportType: sourceReport.ReportType,
                    candidateReportIds: candidateIds,
                    cancellationToken: cancellationToken);

        var distancesByReportId = candidateDistances
            .ToDictionary(
                candidate => candidate.PetReportId,
                candidate => candidate.DistanceMeters);

        var matches = new List<PetReportMatch>();

        foreach (var candidateReport in candidateReports)
        {
            if (existingCounterpartIds.Contains(
                    candidateReport.Id))
            {
                continue;
            }

            if (!distancesByReportId.TryGetValue(
                    candidateReport.Id,
                    out var distanceMeters))
            {
                continue;
            }

            var score =
                PetReportMatchScoreCalculator.Calculate(
                    sourceReport,
                    candidateReport,
                    distanceMeters);

            if (score <
                PetReportMatchScoreCalculator
                    .MinimumSuggestedScore)
            {
                continue;
            }

            var lostReportId =
                sourceReport.ReportType == ReportType.Lost
                    ? sourceReport.Id
                    : candidateReport.Id;

            var foundReportId =
                sourceReport.ReportType == ReportType.Found
                    ? sourceReport.Id
                    : candidateReport.Id;

            var match = PetReportMatch.Create(
                lostReportId,
                foundReportId,
                score,
                distanceMeters);

            matches.Add(match);
        }

        if (matches.Count == 0)
        {
            return;
        }

        await _matchRepository.AddRangeAsync(
            matches,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}