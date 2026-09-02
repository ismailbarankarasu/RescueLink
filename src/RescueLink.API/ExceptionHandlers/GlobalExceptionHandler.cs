using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RescueLink.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const int MaximumMethodLength = 32;
    private const int MaximumPathLength = 2048;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId =
            httpContext.TraceIdentifier;

        var requestMethod =
            SanitizeForLog(
                httpContext.Request.Method,
                MaximumMethodLength);

        var requestPath =
            SanitizeForLog(
                httpContext.Request.Path.Value,
                MaximumPathLength);

        logger.LogError(
            exception,
            "An unhandled exception occurred. " +
            "TraceId: {TraceId}, " +
            "Method: {RequestMethod}, " +
            "Path: {RequestPath}",
            traceId,
            requestMethod,
            requestPath);

        var problemDetails =
            new ProblemDetails
            {
                Status =
                    StatusCodes
                        .Status500InternalServerError,

                Title =
                    "An unexpected error occurred.",

                Detail =
                    "The request could not be " +
                    "completed. Use the trace ID " +
                    "when contacting support.",

                Instance =
                    httpContext.Request.Path
            };

        problemDetails.Extensions["traceId"] =
            traceId;

        httpContext.Response.StatusCode =
            StatusCodes
                .Status500InternalServerError;

        await httpContext.Response
            .WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

        return true;
    }

    private static string SanitizeForLog(
        string? value,
        int maximumLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sanitizedValue =
            value
                .Replace(
                    "\r",
                    string.Empty,
                    StringComparison.Ordinal)
                .Replace(
                    "\n",
                    string.Empty,
                    StringComparison.Ordinal);

        return sanitizedValue.Length <=
               maximumLength
            ? sanitizedValue
            : sanitizedValue[..maximumLength];
    }
}