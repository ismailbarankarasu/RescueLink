using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.Create;

public sealed record CreatePetReportCommand(
    ReportType ReportType,
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
    double Longitude)
    : ICommand<Result<Guid>>;