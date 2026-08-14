using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Persistence.Context;
using RescueLink.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using RescueLink.Persistence.Identity;
using RescueLink.Application.Abstractions.Authentication;

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
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<RescueLinkDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IPetReportRepository, PetReportRepository>();

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<RescueLinkDbContext>());

        return services;
    }
}