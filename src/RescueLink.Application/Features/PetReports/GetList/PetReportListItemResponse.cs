using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.GetList;

public sealed record PetReportListItemResponse(
    Guid Id,
    ReportType ReportType,
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
    string? PrimaryPhotoStorageKey);