using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueLink.Application.Abstractions.Localization;
using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Behaviors;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Features.Notifications;
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
        services.AddScoped<
            IDomainEventDispatcher,
            MediatRDomainEventDispatcher>();


        services.AddValidatorsFromAssembly(assembly);
        services.AddSingleton<INotificationContentLocalizer, NotificationContentLocalizer>();
        services.AddAutoMapper(
            configuration =>
            {},
            assembly);

        return services;
    }
}