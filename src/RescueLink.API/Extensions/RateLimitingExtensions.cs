using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using RescueLink.API.Common;
using RescueLink.Application.Common.Results;

namespace RescueLink.API.Extensions;

public static class RateLimitingExtensions
{
    private const int DefaultAuthenticationPermitLimit = 5;
    private const int DefaultTokenPermitLimit = 10;

    private static readonly TimeSpan DefaultWindow =
        TimeSpan.FromMinutes(1);

    private static readonly Error RateLimitExceeded = new(
        "RateLimit.Exceeded",
        "Too many requests. Please try again later.");

    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            var authenticationPermitLimit =
                 GetPositivePermitLimit(configuration, "RateLimiting:Authentication:PermitLimit", DefaultAuthenticationPermitLimit);

            var tokenPermitLimit =
                GetPositivePermitLimit(configuration, "RateLimiting:Token:PermitLimit", DefaultTokenPermitLimit);
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

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
                                GetIpPartitionKey(
                                    httpContext),
                            factory: _ =>
                                CreateLimiterOptions(
                                    authenticationPermitLimit)));

            options.AddPolicy(
                RateLimitPolicies.Token,
                httpContext =>
                    RateLimitPartition
                        .GetFixedWindowLimiter(
                            partitionKey:
                                GetUserOrIpPartitionKey(
                                    httpContext),
                            factory: _ =>
                                CreateLimiterOptions(
                                    tokenPermitLimit)));
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions
        CreateLimiterOptions(
            int permitLimit)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = DefaultWindow,
            QueueLimit = 0,
            AutoReplenishment = true
        };
    }

    private static string GetUserOrIpPartitionKey(
        HttpContext httpContext)
    {
        if (httpContext.User.Identity?
                .IsAuthenticated == true)
        {
            var userId =
                httpContext.User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue(
                    "sub");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        return GetIpPartitionKey(
            httpContext);
    }

    private static string GetIpPartitionKey(
        HttpContext httpContext)
    {
        var remoteIpAddress =
            httpContext.Connection.RemoteIpAddress;

        if (remoteIpAddress is null)
        {
            return "ip:unknown";
        }

        if (remoteIpAddress.IsIPv4MappedToIPv6)
        {
            remoteIpAddress =
                remoteIpAddress.MapToIPv4();
        }

        return $"ip:{remoteIpAddress}";
    }

    private static int GetPositivePermitLimit(
        IConfiguration configuration,
        string configurationKey,
        int defaultValue)
    {
        var configuredValue =
            configuration.GetValue<int?>(
                configurationKey);

        return configuredValue is > 0
            ? configuredValue.Value
            : defaultValue;
    }
}