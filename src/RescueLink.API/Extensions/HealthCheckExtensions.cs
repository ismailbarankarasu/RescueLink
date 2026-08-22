using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RescueLink.Persistence.Context;

namespace RescueLink.API.Extensions;

public static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddApiHealthChecks(
        this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<RescueLinkDbContext>(
                name: "sql-server",
                failureStatus:
                    HealthStatus.Unhealthy,
                tags: [ReadyTag]);

        return services;
    }

    public static IEndpointRouteBuilder
        MapApiHealthChecks(
            this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = _ => false
            });

        endpoints.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = registration =>
                    registration.Tags.Contains(
                        ReadyTag)
            });

        return endpoints;
    }
}