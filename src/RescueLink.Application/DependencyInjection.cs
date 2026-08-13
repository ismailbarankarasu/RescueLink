using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueLink.Application.Common.Behaviors;
using System.Reflection;

namespace RescueLink.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            configuration.AddOpenBehavior(
                typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(
            configuration =>
            {},
            assembly);

        return services;
    }
}