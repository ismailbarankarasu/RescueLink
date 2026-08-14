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

    public static Error RegistrationFailed(string description)
    {
        return new Error(
            "Authentication.RegistrationFailed",
            description);
    }
}