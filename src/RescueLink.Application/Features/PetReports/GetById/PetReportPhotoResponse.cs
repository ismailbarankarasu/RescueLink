namespace RescueLink.Application.Features.PetReports.GetById;

public sealed record PetReportPhotoResponse(
    Guid Id,
    string StorageKey,
    bool IsPrimary,
    int DisplayOrder);