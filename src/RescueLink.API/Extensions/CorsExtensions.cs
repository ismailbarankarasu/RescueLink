using RescueLink.API.Common;

namespace RescueLink.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins =
            configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
            ?? [];

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one CORS origin must be configured.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(
                CorsPolicies.Frontend,
                policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        return services;
    }
}