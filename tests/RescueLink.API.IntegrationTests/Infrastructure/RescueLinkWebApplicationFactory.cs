using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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