using RescueLink.Application.Features.PetReports.Matching;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Abstractions.Data;

public interface IPetReportMatchCandidateReadService
{
    Task<IReadOnlyCollection<PetReportMatchCandidate>>
        GetCandidatesAsync(
            Guid sourceReportId,
            Guid sourceUserId,
            ReportType candidateReportType,
            AnimalSpecies species,
            double latitude,
            double longitude,
            double maximumDistanceMeters,
            int limit,
            CancellationToken cancellationToken);
}