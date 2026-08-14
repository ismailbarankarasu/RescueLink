namespace RescueLink.Application.Features.PetReports.Nearby;

public sealed class NearbyPetReportResponse
{
    public Guid Id { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string? Breed { get; init; }
    public string PrimaryColor { get; init; } = string.Empty;
    public DateTimeOffset EventDate { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double DistanceMeters { get; init; }
}