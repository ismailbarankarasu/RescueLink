using RescueLink.Application.Common.Results;
using RescueLink.Application.Localization;

namespace RescueLink.API.Common;

internal sealed class ErrorLocalizer
    : IErrorLocalizer
{
    public Error Localize(Error error)
    {
        var localizedMessage = error.Code switch
        {
            "Authentication.InvalidCredentials" =>
                ErrorMessages
                    .AuthenticationInvalidCredentials,

            "Authentication.EmailAlreadyInUse" =>
                ErrorMessages
                    .AuthenticationEmailAlreadyInUse,

            "Authentication.InvalidRefreshToken" =>
                ErrorMessages
                    .AuthenticationInvalidRefreshToken,

            _ => error.Message
        };

        return new Error(
            error.Code,
            localizedMessage);
    }
}