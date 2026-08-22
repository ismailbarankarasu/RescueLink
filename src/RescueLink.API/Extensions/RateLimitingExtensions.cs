using Microsoft.AspNetCore.RateLimiting;
using RescueLink.API.Common;
using System.Threading.RateLimiting;

namespace RescueLink.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (
                context,
                cancellationToken) =>
            {
                context.HttpContext.Response.ContentType =
                    "application/json";

                await context.HttpContext.Response
                    .WriteAsJsonAsync(
                        new
                        {
                            code = "RateLimit.Exceeded",
                            message =
                                "Too many requests. Please try again later."
                        },
                        cancellationToken);
            };

            options.AddPolicy(
                RateLimitPolicies.Authentication,
                httpContext =>
                    RateLimitPartition
                        .GetFixedWindowLimiter(
                            partitionKey:
                                GetPartitionKey(
                                    httpContext),
                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 5,
                                    Window =
                                        TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                }));

            options.AddPolicy(
                RateLimitPolicies.Token,
                httpContext =>
                    RateLimitPartition
                        .GetFixedWindowLimiter(
                            partitionKey:
                                GetPartitionKey(
                                    httpContext),
                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 10,
                                    Window =
                                        TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                }));
        });

        return services;
    }

    private static string GetPartitionKey(
        HttpContext httpContext)
    {
        return httpContext.Connection
                   .RemoteIpAddress?
                   .ToString()
               ?? "unknown";
    }
}