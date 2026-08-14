using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.Nearby;

public sealed record GetNearbyPetReportsQuery(
    double Latitude,
    double Longitude,
    double RadiusMeters,
    ReportType? ReportType,
    AnimalSpecies? Species,
    int Limit = 20)
    : IQuery<Result<IReadOnlyCollection<NearbyPetReportResponse>>>;