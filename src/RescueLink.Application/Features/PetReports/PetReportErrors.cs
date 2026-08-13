using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports;

public static class PetReportErrors
{
    public static readonly Error Unauthenticated = new(
        "Authentication.Unauthenticated",
        "The current user is not authenticated.");

    public static Error NotFound(Guid reportId)
    {
        return new Error(
            "PetReport.NotFound",
            $"Pet report '{reportId}' was not found.");
    }
}