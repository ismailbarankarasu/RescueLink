using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RescueLink.API.IntegrationTests.Infrastructure;

namespace RescueLink.API.IntegrationTests.Features.Users;

public sealed class UserProfileEndpointTests
    : IClassFixture<SqlServerContainerFixture>
{
    private readonly SqlServerContainerFixture
        _sqlServerContainer;

    public UserProfileEndpointTests(
        SqlServerContainerFixture sqlServerContainer)
    {
        _sqlServerContainer = sqlServerContainer;
    }

    [Fact]
    public async Task UpdateAndGetMe_ShouldReturnUpdatedProfile()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"profile-{Guid.NewGuid():N}@example.com";

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

        // Act - Profili güncelle
        var updateResponse =
            await client.PutAsJsonAsync(
                "/api/users/me",
                new
                {
                    FirstName = "İsmail Baran",
                    LastName = "Karasu",
                    PhoneNumber = "+905551234567",
                    CountryCode = "tr",
                    City = "Bursa",
                    PreferredLanguage = "tr",
                    TimeZoneId = "Europe/Istanbul"
                });

        var updateBody =
            await updateResponse.Content
                .ReadAsStringAsync();

        // Assert - Güncelleme başarılı
        updateResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"update response: {updateBody}");

        // Act - Güncel profili getir
        var getResponse =
            await client.GetAsync(
                "/api/users/me");

        var getBody =
            await getResponse.Content
                .ReadAsStringAsync();

        getResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"get response: {getBody}");

        using var getJson =
            JsonDocument.Parse(getBody);

        var profile = getJson.RootElement;

        profile.GetProperty("firstName")
            .GetString()
            .Should()
            .Be("İsmail Baran");

        profile.GetProperty("lastName")
            .GetString()
            .Should()
            .Be("Karasu");

        profile.GetProperty("email")
            .GetString()
            .Should()
            .Be(email);

        profile.GetProperty("phoneNumber")
            .GetString()
            .Should()
            .Be("+905551234567");

        profile.GetProperty("countryCode")
            .GetString()
            .Should()
            .Be("TR");

        profile.GetProperty("city")
            .GetString()
            .Should()
            .Be("Bursa");

        profile.GetProperty("preferredLanguage")
            .GetString()
            .Should()
            .Be("tr");

        profile.GetProperty("timeZoneId")
            .GetString()
            .Should()
            .Be("Europe/Istanbul");
        // Act - Geçersiz profil güncellemesi
        var invalidUpdateResponse =
            await client.PutAsJsonAsync(
                "/api/users/me",
                new
                {
                    FirstName = "",
                    LastName = "",
                    PhoneNumber = "05551234567",
                    CountryCode = "TUR",
                    City = "İstanbul",
                    PreferredLanguage =
                        "invalid-language-code",
                    TimeZoneId = "Mars/Olympus"
                });

        var invalidUpdateBody =
            await invalidUpdateResponse.Content
                .ReadAsStringAsync();

        // Assert - Validation isteği reddetti
        invalidUpdateResponse.StatusCode.Should().Be(
            HttpStatusCode.BadRequest,
            $"invalid update response: {invalidUpdateBody}");

        using var invalidJson =
            JsonDocument.Parse(invalidUpdateBody);

        invalidJson.RootElement
            .TryGetProperty("errors", out var errors)
            .Should()
            .BeTrue();

        errors.TryGetProperty(
                "FirstName",
                out _)
            .Should()
            .BeTrue();

        errors.TryGetProperty(
                "CountryCode",
                out _)
            .Should()
            .BeTrue();

        errors.TryGetProperty(
                "TimeZoneId",
                out _)
            .Should()
            .BeTrue();

        // Act - Profili tekrar getir
        var unchangedResponse =
            await client.GetAsync(
                "/api/users/me");

        var unchangedBody =
            await unchangedResponse.Content
                .ReadAsStringAsync();

        unchangedResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"unchanged response: {unchangedBody}");

        using var unchangedJson =
            JsonDocument.Parse(unchangedBody);

        var unchangedProfile =
            unchangedJson.RootElement;

        unchangedProfile.GetProperty("firstName")
            .GetString()
            .Should()
            .Be("İsmail Baran");

        unchangedProfile.GetProperty("countryCode")
            .GetString()
            .Should()
            .Be("TR");

        unchangedProfile.GetProperty("city")
            .GetString()
            .Should()
            .Be("Bursa");

        unchangedProfile.GetProperty("timeZoneId")
            .GetString()
            .Should()
            .Be("Europe/Istanbul");
    }
}