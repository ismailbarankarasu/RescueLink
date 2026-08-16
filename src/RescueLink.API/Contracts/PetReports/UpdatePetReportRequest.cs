using RescueLink.Domain.Enums;

namespace RescueLink.API.Contracts.PetReports;

public sealed record UpdatePetReportRequest(
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
    double Longitude);