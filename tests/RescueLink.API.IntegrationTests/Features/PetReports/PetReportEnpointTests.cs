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

        // Arrange - Merkezden yaklaşık 550 metre uzakta ikinci ilan
        var secondCreateResponse =
            await client.PostAsJsonAsync(
                "/api/pet-reports",
                new
                {
                    ReportType = "Lost",
                    Title = "Daha uzaktaki kayıp kedi",
                    Description =
                        "Spatial sıralama testi için oluşturulan ikinci ilan.",
                    Species = "Cat",
                    Gender = "Male",
                    PetName = "Atlas",
                    Breed = "Tekir",
                    PrimaryColor = "Gray",
                    SecondaryColor = "White",
                    EventDate =
                        DateTimeOffset.UtcNow.AddHours(-2),
                    Latitude = 40.200,
                    Longitude = 29.060
                });

        var secondCreateBody =
            await secondCreateResponse.Content
                .ReadAsStringAsync();

        secondCreateResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"second create response: {secondCreateBody}");

        using var secondCreateJson =
            JsonDocument.Parse(secondCreateBody);

        var secondReportId =
            secondCreateJson.RootElement
                .GetProperty("petReportId")
                .GetGuid();

        secondReportId.Should().NotBe(Guid.Empty);

        // Act - Merkez noktanın çevresindeki ilanları getir
        var nearbyResponse =
            await client.GetAsync(
                "/api/pet-reports/nearby" +
                "?latitude=40.195" +
                "&longitude=29.060" +
                "&radiusMeters=2000" +
                "&reportType=Lost" +
                "&species=Cat" +
                "&limit=20");

        var nearbyBody =
            await nearbyResponse.Content
                .ReadAsStringAsync();

        // Assert - Spatial sorgu başarılı
        nearbyResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"nearby response: {nearbyBody}");

        using var nearbyJson =
            JsonDocument.Parse(nearbyBody);

        var relevantReports =
            nearbyJson.RootElement
                .EnumerateArray()
                .Where(item =>
                {
                    var id =
                        item.GetProperty("id").GetGuid();

                    return id == reportId ||
                           id == secondReportId;
                })
                .ToArray();

        relevantReports.Should().HaveCount(2);

        // En yakın ilan ilk sırada olmalı
        relevantReports[0]
            .GetProperty("id")
            .GetGuid()
            .Should()
            .Be(reportId);

        // Daha uzaktaki ilan ikinci sırada olmalı
        relevantReports[1]
            .GetProperty("id")
            .GetGuid()
            .Should()
            .Be(secondReportId);

        var firstDistance =
            relevantReports[0]
                .GetProperty("distanceMeters")
                .GetDouble();

        var secondDistance =
            relevantReports[1]
                .GetProperty("distanceMeters")
                .GetDouble();

        firstDistance.Should()
            .BeApproximately(0, 0.1);

        secondDistance.Should()
            .BeGreaterThan(500);

        secondDistance.Should()
            .BeLessThan(600);

        firstDistance.Should()
            .BeLessThan(secondDistance);

        // Act - İlanı arşivle
        var archiveResponse =
            await client.DeleteAsync(
                $"/api/pet-reports/{reportId}");

        var archiveBody =
            await archiveResponse.Content
                .ReadAsStringAsync();

        // Assert - Arşivleme başarılı
        archiveResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"archive response: {archiveBody}");

        // Act - Aynı ilanı tekrar arşivle
        var secondArchiveResponse =
            await client.DeleteAsync(
                $"/api/pet-reports/{reportId}");

        // Assert - İşlem idempotent
        secondArchiveResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act - Arşivlenmiş ilanı ID ile getir
        var archivedGetResponse =
            await client.GetAsync(
                $"/api/pet-reports/{reportId}");

        // Assert - Arşivlenen ilan artık görünmüyor
        archivedGetResponse.StatusCode.Should().Be(
            HttpStatusCode.NotFound);

        // Act - Nearby sorgusunu tekrar çalıştır
        var archivedNearbyResponse =
            await client.GetAsync(
                "/api/pet-reports/nearby" +
                "?latitude=40.195" +
                "&longitude=29.060" +
                "&radiusMeters=1000" +
                "&reportType=Lost" +
                "&species=Cat" +
                "&limit=20");

        var archivedNearbyBody =
            await archivedNearbyResponse.Content
                .ReadAsStringAsync();

        archivedNearbyResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"nearby response after archive: " +
            archivedNearbyBody);

        using var archivedNearbyJson =
            JsonDocument.Parse(
                archivedNearbyBody);

        var containsArchivedReport =
            archivedNearbyJson.RootElement
                .EnumerateArray()
                .Any(item =>
                    item.GetProperty("id").GetGuid() ==
                    reportId);

        containsArchivedReport.Should().BeFalse();

        // Act - Normal kullanıcı ilanlarını getir
        var activeMineResponse =
            await client.GetAsync(
                "/api/pet-reports/mine");

        var activeMineBody =
            await activeMineResponse.Content
                .ReadAsStringAsync();

        // Assert - Arşivlenen ilan normal listede bulunmuyor
        activeMineResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"active mine response: {activeMineBody}");

        using var activeMineJson =
            JsonDocument.Parse(activeMineBody);

        var activeMineContainsArchivedReport =
            activeMineJson.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Any(item =>
                    item.GetProperty("id").GetGuid() ==
                    reportId);

        activeMineContainsArchivedReport.Should().BeFalse();

        // Act - Arşivlenmiş kullanıcı ilanlarını getir
        var archivedMineResponse =
            await client.GetAsync(
                "/api/pet-reports/mine" +
                "?archivedOnly=true");

        var archivedMineBody =
            await archivedMineResponse.Content
                .ReadAsStringAsync();

        // Assert - Arşivlenen ilan arşiv listesinde bulunuyor
        archivedMineResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"archived mine response: {archivedMineBody}");

        using var archivedMineJson =
            JsonDocument.Parse(archivedMineBody);

        var archivedMineContainsReport =
            archivedMineJson.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Any(item =>
                    item.GetProperty("id").GetGuid() ==
                    reportId);

        archivedMineContainsReport.Should().BeTrue();

        // Act - Arşivlenen ilanı geri yükle
        var restoreResponse =
            await client.PatchAsync(
                $"/api/pet-reports/{reportId}/restore",
                content: null);

        var restoreBody =
            await restoreResponse.Content
                .ReadAsStringAsync();

        // Assert - Restore başarılı
        restoreResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"restore response: {restoreBody}");

        // Act - Aynı ilanı tekrar geri yükle
        var secondRestoreResponse =
            await client.PatchAsync(
                $"/api/pet-reports/{reportId}/restore",
                content: null);

        // Assert - Restore işlemi idempotent
        secondRestoreResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act - Geri yüklenen ilanı ID ile getir
        var restoredGetResponse =
            await client.GetAsync(
                $"/api/pet-reports/{reportId}");

        var restoredGetBody =
            await restoredGetResponse.Content
                .ReadAsStringAsync();

        // Assert - İlan yeniden erişilebilir
        restoredGetResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"restored get response: {restoredGetBody}");

        using var restoredGetJson =
            JsonDocument.Parse(restoredGetBody);

        restoredGetJson.RootElement
            .GetProperty("id")
            .GetGuid()
            .Should()
            .Be(reportId);

        // Act - Arşiv listesini tekrar getir
        var archivedMineAfterRestoreResponse =
            await client.GetAsync(
                "/api/pet-reports/mine" +
                "?archivedOnly=true");

        var archivedMineAfterRestoreBody =
            await archivedMineAfterRestoreResponse.Content
                .ReadAsStringAsync();

        archivedMineAfterRestoreResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"archived mine after restore response: " +
            archivedMineAfterRestoreBody);

        using var archivedMineAfterRestoreJson =
            JsonDocument.Parse(
                archivedMineAfterRestoreBody);

        // Assert - Restore edilen ilan arşiv listesinden çıktı
        var archiveStillContainsRestoredReport =
            archivedMineAfterRestoreJson.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Any(item =>
                    item.GetProperty("id").GetGuid() ==
                    reportId);

        archiveStillContainsRestoredReport.Should().BeFalse();

        // Act - Normal ilan listesini tekrar getir
        var activeMineAfterRestoreResponse =
            await client.GetAsync(
                "/api/pet-reports/mine");

        var activeMineAfterRestoreBody =
            await activeMineAfterRestoreResponse.Content
                .ReadAsStringAsync();

        activeMineAfterRestoreResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"active mine after restore response: " +
            activeMineAfterRestoreBody);

        using var activeMineAfterRestoreJson =
            JsonDocument.Parse(
                activeMineAfterRestoreBody);

        // Assert - Restore edilen ilan normal listeye döndü
        var activeMineContainsRestoredReport =
            activeMineAfterRestoreJson.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Any(item =>
                    item.GetProperty("id").GetGuid() ==
                    reportId);

        activeMineContainsRestoredReport.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var createRequest = new
        {
            ReportType = "Lost",
            Title = "Yetkisiz ilan denemesi",
            Description =
                "JWT olmadan oluşturulmaya çalışılan test ilanı.",
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

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/pet-reports",
                createRequest);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnLocalizedNotFound_WhenReportDoesNotExist()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders
            .AcceptLanguage
            .ParseAdd("tr");

        var reportId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync(
            $"/api/pet-reports/{reportId}");

        var responseBody =
            await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound,
            $"response: {responseBody}");

        using var responseJson =
            JsonDocument.Parse(responseBody);

        var error = responseJson.RootElement;

        error.GetProperty("code")
            .GetString()
            .Should()
            .Be("PetReport.NotFound");

        error.GetProperty("message")
            .GetString()
            .Should()
            .Be("Hayvan ilanı bulunamadı.");
    }
}