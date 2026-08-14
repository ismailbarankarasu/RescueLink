namespace RescueLink.API.Contracts.PetReports;

public sealed class UploadPetReportPhotoRequest
{
    public required IFormFile File { get; init; }
}