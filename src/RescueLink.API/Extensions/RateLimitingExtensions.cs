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

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            var authenticationPermitLimit = configuration.GetValue<int?>("RateLimiting:Authentication:PermitLimit") ?? 5;

            var tokenPermitLimit = configuration.GetValue<int?>("RateLimiting:Token:PermitLimit") ?? 10;

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
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
                        .GetRequiredService<IErrorLocalizer>();

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
                                GetPartitionKey(httpContext),
                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit =
                                        authenticationPermitLimit,

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
                                GetPartitionKey(httpContext),
                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit =
                                        tokenPermitLimit,

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