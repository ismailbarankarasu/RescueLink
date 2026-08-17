using Testcontainers.MsSql;

namespace RescueLink.API.IntegrationTests.Infrastructure;

public sealed class SqlServerContainerFixture
    : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder()
            .WithImage(
                "mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

    public string ConnectionString =>
        _container.GetConnectionString();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container
            .DisposeAsync()
            .AsTask();
    }
}