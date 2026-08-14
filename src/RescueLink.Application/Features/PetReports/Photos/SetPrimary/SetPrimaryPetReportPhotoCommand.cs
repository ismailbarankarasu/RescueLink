using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Photos.SetPrimary;

public sealed record SetPrimaryPetReportPhotoCommand(
    Guid PetReportId,
    Guid PhotoId)
    : ICommand<Result>;