using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.GetById;

public sealed record PetReportDetailResponse(
    Guid Id,
    Guid UserId,
    ReportType ReportType,
    ReportStatus Status,
    string Title,
    string Description,
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
    IReadOnlyCollection<PetReportPhotoResponse> Photos);