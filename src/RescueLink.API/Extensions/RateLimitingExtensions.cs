using RescueLink.API.Common;
using RescueLink.Application.Common.Results;
using System.Globalization;
using System.Threading.RateLimiting;
namespace RescueLink.API.Extensions;

public static class RateLimitingExtensions
{
    private static readonly Error RateLimitExceeded = new(
        "RateLimit.Exceeded",
        "Too many requests. Please try again later.");

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
                var httpContext =
                    context.HttpContext;

                var response =
                    httpContext.Response;

                response.ContentType =
                    "application/json";

                response.Headers.ContentLanguage =
                    CultureInfo.CurrentUICulture.Name;

                var errorLocalizer =
                    httpContext.RequestServices
                        .GetRequiredService<
                            IErrorLocalizer>();

                var localizedError =
                    errorLocalizer.Localize(
                        RateLimitExceeded);

                await response.WriteAsJsonAsync(
                    localizedError,
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