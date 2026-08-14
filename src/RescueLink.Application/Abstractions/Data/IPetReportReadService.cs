using RescueLink.Application.Features.PetReports.Nearby;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Abstractions.Data;

public interface IPetReportReadService
{
    Task<IReadOnlyCollection<NearbyPetReportResponse>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        ReportType? reportType,
        AnimalSpecies? species,
        int limit,
        CancellationToken cancellationToken);
}