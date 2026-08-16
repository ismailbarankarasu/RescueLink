using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.GetMine;

public sealed record MyPetReportListItemResponse(
    Guid Id,
    ReportType ReportType,
    ReportStatus Status,
    string Title,
    AnimalSpecies Species,
    AnimalGender Gender,
    string? PetName,
    string? Breed,
    AnimalColor PrimaryColor,
    AnimalColor? SecondaryColor,
    DateTimeOffset EventDate,
    double Latitude,
    double Longitude,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? PrimaryPhotoStorageKey);