using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;

public sealed record PetReportMatchResponse(
    Guid MatchId,
    Guid CounterpartReportId,
    ReportType ReportType,
    string Title,
    AnimalSpecies Species,
    AnimalGender Gender,
    string? Breed,
    AnimalColor PrimaryColor,
    AnimalColor? SecondaryColor,
    DateTimeOffset EventDate,
    double Latitude,
    double Longitude,
    int Score,
    double DistanceMeters,
    MatchStatus Status,
    string? PrimaryPhotoStorageKey);