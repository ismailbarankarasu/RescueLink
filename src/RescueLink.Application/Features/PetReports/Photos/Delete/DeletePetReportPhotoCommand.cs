using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Photos.Delete;

public sealed record DeletePetReportPhotoCommand(
    Guid PetReportId,
    Guid PhotoId)
    : ICommand<Result>;