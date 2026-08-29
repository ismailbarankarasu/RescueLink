using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RescueLink.API.IntegrationTests.Infrastructure;

public sealed class RescueLinkWebApplicationFactory(
    string connectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            "IntegrationTesting");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                configurationBuilder
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            [
                                "RateLimiting:" +
                                "Authentication:" +
                                "PermitLimit"
                            ] = "1000",

                            [
                                "RateLimiting:" +
                                "Token:" +
                                "PermitLimit"
                            ] = "1000"
                        });
            });

        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            connectionString);

        builder.UseSetting(
            "Database:ApplyMigrations",
            "true");

        builder.UseSetting(
            "Jwt:SecretKey",
            "IntegrationTestsSecretKey1234567890" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456");

        builder.UseSetting(
            "Jwt:Issuer",
            "RescueLink.IntegrationTests");

        builder.UseSetting(
            "Jwt:Audience",
            "RescueLink.IntegrationTests");

        builder.UseSetting(
            "Jwt:ExpirationMinutes",
            "60");

        builder.UseSetting(
            "Jwt:RefreshTokenExpirationDays",
            "7");

        builder.UseSetting(
            "Cors:AllowedOrigins:0",
            "http://localhost");
    }
}