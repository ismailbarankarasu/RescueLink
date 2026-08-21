using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Users;

public static class UserProfileErrors
{
    public static readonly Error Unauthenticated = new(
        "Authentication.Unauthenticated",
        "The current user is not authenticated.");

    public static readonly Error NotFound = new(
        "UserProfile.NotFound",
        "The user profile was not found.");
    public static Error UpdateFailed(string description)
    {
        return new Error(
            "UserProfile.UpdateFailed",
            description);
    }
}