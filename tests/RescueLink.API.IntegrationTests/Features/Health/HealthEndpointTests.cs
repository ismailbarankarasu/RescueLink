using System.Net;
using FluentAssertions;
using RescueLink.API.IntegrationTests.Infrastructure;

namespace RescueLink.API.IntegrationTests.Features.Health;

public sealed class HealthEndpointTests
    : IClassFixture<SqlServerContainerFixture>
{
    private readonly SqlServerContainerFixture
        _sqlServerContainer;

    public HealthEndpointTests(
        SqlServerContainerFixture sqlServerContainer)
    {
        _sqlServerContainer = sqlServerContainer;
    }

    [Fact]
    public async Task Live_ShouldReturnOk()
    {
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/health/live");

        response.StatusCode.Should().Be(
            HttpStatusCode.OK);

        var content =
            await response.Content.ReadAsStringAsync();

        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Ready_ShouldReturnOk_WhenDatabaseIsAvailable()
    {
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/health/ready");

        response.StatusCode.Should().Be(
            HttpStatusCode.OK);

        var content =
            await response.Content.ReadAsStringAsync();

        content.Should().Contain("Healthy");
    }
}