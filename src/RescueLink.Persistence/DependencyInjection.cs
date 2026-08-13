using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Persistence.Context;
using RescueLink.Persistence.Repositories;

namespace RescueLink.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "DefaultConnection");

        services.AddDbContext<RescueLinkDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                    sqlServerOptions.UseNetTopologySuite()));

        services.AddScoped<IPetReportRepository, PetReportRepository>();

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<RescueLinkDbContext>());

        return services;
    }
}