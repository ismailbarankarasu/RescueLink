using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Photos.Upload;

public sealed record UploadPetReportPhotoCommand(
    Guid PetReportId,
    Stream Content,
    string FileName,
    string ContentType,
    long Length)
    : ICommand<Result<Guid>>;