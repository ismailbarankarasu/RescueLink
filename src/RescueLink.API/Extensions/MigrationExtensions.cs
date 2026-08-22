using Microsoft.EntityFrameworkCore;
using RescueLink.Persistence.Context;

namespace RescueLink.API.Extensions;

public static class MigrationExtensions
{
    public static async Task
        ApplyDatabaseMigrationsAsync(
            this WebApplication app)
    {
        var shouldApplyMigrations =
            app.Configuration.GetValue<bool>(
                "Database:ApplyMigrations");

        if (!shouldApplyMigrations)
        {
            return;
        }

        await using var scope =
            app.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    RescueLinkDbContext>();

        await dbContext.Database
            .MigrateAsync();
    }
}