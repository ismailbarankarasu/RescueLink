using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RescueLink.API.Common;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication;

namespace RescueLink.API.Extensions;

public static class JwtBearerExtensions
{
    public static IServiceCollection
        AddLocalizedJwtResponses(
            this IServiceCollection services)
    {
        services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.Events.OnChallenge =
                    async context =>
                    {
                        context.HandleResponse();

                        await WriteLocalizedErrorAsync(
                            context.HttpContext,
                            AuthenticationErrors
                                .Unauthenticated,
                            StatusCodes
                                .Status401Unauthorized);
                    };

                options.Events.OnForbidden =
                    async context =>
                    {
                        await WriteLocalizedErrorAsync(
                            context.HttpContext,
                            AuthenticationErrors.Forbidden,
                            StatusCodes
                                .Status403Forbidden);
                    };
            });

        return services;
    }

    private static async Task
        WriteLocalizedErrorAsync(
            HttpContext httpContext,
            Error error,
            int statusCode)
    {
        var response = httpContext.Response;

        response.StatusCode = statusCode;
        response.ContentType = "application/json";

        response.Headers.ContentLanguage =
            CultureInfo.CurrentUICulture.Name;

        var errorLocalizer =
            httpContext.RequestServices
                .GetRequiredService<IErrorLocalizer>();

        var localizedError =
            errorLocalizer.Localize(error);

        await response.WriteAsJsonAsync(
            localizedError,
            httpContext.RequestAborted);
    }
}