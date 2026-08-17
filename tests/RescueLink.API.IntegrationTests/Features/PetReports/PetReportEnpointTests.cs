using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RescueLink.API.IntegrationTests.Infrastructure;

namespace RescueLink.API.IntegrationTests
    .Features.PetReports;

public sealed class PetReportEndpointTests
    : IClassFixture<SqlServerContainerFixture>
{
    private readonly SqlServerContainerFixture
        _sqlServerContainer;

    public PetReportEndpointTests(
        SqlServerContainerFixture sqlServerContainer)
    {
        _sqlServerContainer = sqlServerContainer;
    }

    [Fact]
    public async Task CreateAndGetById_ShouldReturnCreatedReport()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"report-{Guid.NewGuid():N}@example.com";

        var password = "Password123";

        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    FirstName = "İsmail",
                    LastName = "Karasu",
                    Email = email,
                    Password = password,
                    ConfirmPassword = password
                });

        var registerBody =
            await registerResponse.Content
                .ReadAsStringAsync();

        registerResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"register response: {registerBody}");

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    Email = email,
                    Password = password
                });

        var loginBody =
            await loginResponse.Content
                .ReadAsStringAsync();

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"login response: {loginBody}");

        using var loginJson =
            JsonDocument.Parse(loginBody);

        var accessToken =
            loginJson.RootElement
                .GetProperty("accessToken")
                .GetString();

        accessToken.Should()
            .NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var createRequest = new
        {
            ReportType = "Lost",
            Title = "Entegrasyon testi kayıp kedi",
            Description =
                "Bu ilan gerçek HTTP ve SQL Server entegrasyon testiyle oluşturuldu.",
            Species = "Cat",
            Gender = "Female",
            PetName = "Luna",
            Breed = "Tekir",
            PrimaryColor = "Gray",
            SecondaryColor = "White",
            EventDate =
                DateTimeOffset.UtcNow.AddHours(-1),
            Latitude = 40.195,
            Longitude = 29.060
        };

        // Act - İlan oluştur
        var createResponse =
            await client.PostAsJsonAsync(
                "/api/pet-reports",
                createRequest);

        var createBody =
            await createResponse.Content
                .ReadAsStringAsync();

        // Assert - İlan oluşturuldu
        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"create response: {createBody}");

        using var createJson =
            JsonDocument.Parse(createBody);

        var reportId =
            createJson.RootElement
                .GetProperty("petReportId")
                .GetGuid();

        reportId.Should().NotBe(Guid.Empty);

        // Act - İlanı getir
        var getResponse =
            await client.GetAsync(
                $"/api/pet-reports/{reportId}");

        var getBody =
            await getResponse.Content
                .ReadAsStringAsync();

        // Assert - Kaydedilen bilgiler doğru
        getResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"get response: {getBody}");

        using var getJson =
            JsonDocument.Parse(getBody);

        var report =
            getJson.RootElement;

        report.GetProperty("id")
            .GetGuid()
            .Should()
            .Be(reportId);

        report.GetProperty("title")
            .GetString()
            .Should()
            .Be("Entegrasyon testi kayıp kedi");

        report.GetProperty("reportType")
            .GetString()
            .Should()
            .Be("Lost");

        report.GetProperty("status")
            .GetString()
            .Should()
            .Be("Active");

        report.GetProperty("latitude")
            .GetDouble()
            .Should()
            .BeApproximately(40.195, 0.000001);

        report.GetProperty("longitude")
            .GetDouble()
            .Should()
            .BeApproximately(29.060, 0.000001);
    }
}