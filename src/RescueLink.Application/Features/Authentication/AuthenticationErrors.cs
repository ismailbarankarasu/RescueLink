using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Authentication;

public static class AuthenticationErrors
{
    public static readonly Error EmailAlreadyInUse = new(
        "Authentication.EmailAlreadyInUse",
        "This email address is already in use.");

    public static readonly Error InvalidCredentials = new(
        "Authentication.InvalidCredentials",
        "Email or password is incorrect.");

    public static readonly Error InvalidRefreshToken = new(
        "Authentication.InvalidRefreshToken",
        "The refresh token is invalid, expired or revoked.");

    public static Error RegistrationFailed(string description)
    {
        return new Error(
            "Authentication.RegistrationFailed",
            description);
    }
    public static readonly Error Unauthenticated = new(
        "Authentication.Unauthenticated",
        "The current user is not authenticated.");

    public static readonly Error Forbidden = new(
        "Authorization.Forbidden",
        "You do not have permission to perform this operation.");
}