using System.Text.Json.Serialization;
using RescueLink.API.Common;
using RescueLink.API.ExceptionHandlers;

namespace RescueLink.API.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions
                    .Converters
                    .Add(
                        new JsonStringEnumConverter());
            });

        services.AddOpenApi();

        services.AddSingleton<
            IErrorLocalizer,
            ErrorLocalizer>();

        services.AddProblemDetails();

        services.AddExceptionHandler<
            ValidationExceptionHandler>();

        services.AddExceptionHandler<
            GlobalExceptionHandler>();

        return services;
    }
}