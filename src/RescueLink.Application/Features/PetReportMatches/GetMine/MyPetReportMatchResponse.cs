using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReportMatches.GetMine;

public sealed record MyPetReportMatchResponse(
    Guid MatchId,
    Guid SourceReportId,
    ReportType SourceReportType,
    string SourceReportTitle,
    Guid CounterpartReportId,
    ReportType CounterpartReportType,
    string CounterpartReportTitle,
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
    bool CurrentUserConfirmed,
    bool CounterpartConfirmed,
    string? PrimaryPhotoStorageKey);