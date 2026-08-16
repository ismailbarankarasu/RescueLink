using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReportMatches;

public static class PetReportMatchErrors
{
    public static readonly Error Unauthenticated = new(
        "Authentication.Unauthenticated",
        "The current user is not authenticated.");

    public static readonly Error Forbidden = new(
        "PetReportMatch.Forbidden",
        "You are not allowed to manage this match.");

    public static readonly Error NotSuggested = new(
        "PetReportMatch.NotSuggested",
        "Only suggested matches can be modified.");
    public static readonly Error ReportsNotActive = new(
        "PetReportMatch.ReportsNotActive",
        "Both pet reports must be active to confirm the match.");
    public static Error NotFound(Guid matchId)
    {
        return new Error(
            "PetReportMatch.NotFound",
            $"Pet report match '{matchId}' was not found.");
    }
}