namespace RescueLink.Application.Features.PetReports.Matching;

public sealed record PetReportMatchCandidate(
    Guid PetReportId,
    double DistanceMeters);