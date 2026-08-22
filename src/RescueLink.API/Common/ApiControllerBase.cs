using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Common.Results;

namespace RescueLink.API.Common;

public abstract class ApiControllerBase
    : ControllerBase
{
    private readonly IErrorLocalizer
        _errorLocalizer;

    protected ApiControllerBase(
        IErrorLocalizer errorLocalizer)
    {
        _errorLocalizer = errorLocalizer;
    }

    protected IActionResult HandleFailure(Error error, int? statusCode = null)
    {
        var localizedError =
            _errorLocalizer.Localize(error);

        var resolvedStatusCode =
            statusCode ??
            GetStatusCode(error.Code);

        return StatusCode(
            resolvedStatusCode,
            localizedError);
    }

    private static int GetStatusCode(
        string errorCode)
    {
        return errorCode switch
        {
            "Authentication.InvalidCredentials" =>
                StatusCodes.Status401Unauthorized,

            "Authentication.InvalidRefreshToken" =>
                StatusCodes.Status401Unauthorized,

            "Authentication.EmailAlreadyInUse" =>
                StatusCodes.Status409Conflict,

            _ when errorCode.EndsWith(
                ".Unauthenticated",
                StringComparison.Ordinal) =>
                    StatusCodes.Status401Unauthorized,

            _ when errorCode.EndsWith(
                ".Forbidden",
                StringComparison.Ordinal) =>
                    StatusCodes.Status403Forbidden,

            _ when errorCode.EndsWith(
                ".NotFound",
                StringComparison.Ordinal) =>
                    StatusCodes.Status404NotFound,

            _ when errorCode.EndsWith(
                ".NotActive",
                StringComparison.Ordinal) =>
                    StatusCodes.Status409Conflict,

            _ when errorCode.EndsWith(
                ".ContactNotAvailable",
                StringComparison.Ordinal) =>
                    StatusCodes.Status409Conflict,
            _ when errorCode.EndsWith(
                ".NotSuggested",
                StringComparison.Ordinal) =>
                    StatusCodes.Status409Conflict,

            _ when errorCode.EndsWith(
                ".ReportsNotActive",
                StringComparison.Ordinal) =>
                    StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status400BadRequest
        };
    }
}