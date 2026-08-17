using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RescueLink.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "An unhandled exception occurred. " +
            "TraceId: {TraceId}, Method: {RequestMethod}, Path: {RequestPath}",
            traceId,
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail =
                "The request could not be completed. " +
                "Use the trace ID when contacting support.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}