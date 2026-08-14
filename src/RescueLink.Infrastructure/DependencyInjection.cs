using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Infrastructure.Authentication;
using System.Security.Claims;
using System.Text;

namespace RescueLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(
            JwtSettings.SectionName);

        var issuer = jwtSection[nameof(JwtSettings.Issuer)];
        var audience = jwtSection[nameof(JwtSettings.Audience)];
        var secretKey = jwtSection[nameof(JwtSettings.SecretKey)];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "JWT settings are missing.");
        }

        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException(
                "JWT secret key must be at least 32 bytes.");
        }

        services.Configure<JwtSettings>(jwtSection);

        services.AddScoped<
            IJwtTokenGenerator,
            JwtTokenGenerator>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(secretKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        NameClaimType =
                            ClaimTypes.NameIdentifier,

                        RoleClaimType =
                            ClaimTypes.Role
                    };
            });

        services.AddAuthorization();

        return services;
    }
}