using System.Security.Claims;
using Serilog;
using Serilog.Events;

namespace RescueLink.API.Extensions;

public static class SerilogExtensions
{
    public static ConfigureHostBuilder AddApiSerilog(
        this ConfigureHostBuilder host)
    {
        host.UseSerilog(
            (context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(
                        context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });

        return host;
    }

    public static WebApplication
        UseApiRequestLogging(
            this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} " +
                "responded {StatusCode} " +
                "in {Elapsed:0.0000} ms | " +
                "TraceId: {TraceId} | UserId: {UserId}";

            options.GetLevel = (
                httpContext,
                elapsed,
                exception) =>
            {
                if (exception is not null ||
                    httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                return LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (
                diagnosticContext,
                httpContext) =>
            {
                var userId =
                    httpContext.User.FindFirst(
                        ClaimTypes.NameIdentifier)
                        ?.Value
                    ?? "Anonymous";

                diagnosticContext.Set(
                    "TraceId",
                    httpContext.TraceIdentifier);

                diagnosticContext.Set(
                    "UserId",
                    userId);
            };
        });

        return app;
    }
}