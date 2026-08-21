using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Localization;

namespace RescueLink.API.ExceptionHandlers;

public sealed class ValidationExceptionHandler
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException
            validationException)
        {
            return false;
        }

        var errors = validationException.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error =>
                        error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var problemDetails =
            new ValidationProblemDetails(errors)
            {
                Status =
                    StatusCodes.Status400BadRequest,

                Title =
                    ValidationMessages
                        .ValidationFailedTitle,

                Detail =
                    ValidationMessages
                        .ValidationFailedDetail,

                Instance =
                    httpContext.Request.Path
            };

        httpContext.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}