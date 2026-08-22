using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace RescueLink.API.Extensions;

public static class LocalizationExtensions
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        new CultureInfo("en"),
        new CultureInfo("tr"),
        new CultureInfo("de")
    ];

    public static IServiceCollection AddApiLocalization(
        this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(
            options =>
            {
                options.DefaultRequestCulture =
                    new RequestCulture("en");

                options.SupportedCultures =
                    SupportedCultures;

                options.SupportedUICultures =
                    SupportedCultures;

                options.ApplyCurrentCultureToResponseHeaders =
                    true;

                options.RequestCultureProviders =
                [
                    new AcceptLanguageHeaderRequestCultureProvider()
                ];
            });

        return services;
    }
}