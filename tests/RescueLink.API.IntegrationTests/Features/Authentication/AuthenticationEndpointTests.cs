using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RescueLink.API.IntegrationTests.Infrastructure;

namespace RescueLink.API.IntegrationTests
    .Features.Authentication;

public sealed class AuthenticationEndpointTests
    : IClassFixture<SqlServerContainerFixture>
{
    private readonly SqlServerContainerFixture
        _sqlServerContainer;

    public AuthenticationEndpointTests(
        SqlServerContainerFixture sqlServerContainer)
    {
        _sqlServerContainer = sqlServerContainer;
    }

    [Fact]
    public async Task RegisterAndLogin_ShouldReturnTokens()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"integration-{Guid.NewGuid():N}@example.com";

        var registerRequest = new
        {
            FirstName = "İsmail",
            LastName = "Karasu",
            Email = email,
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        // Act - Register
        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        // Assert - Register
        var registerResponseBody =
            await registerResponse.Content
            .ReadAsStringAsync();

        registerResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"register response: {registerResponseBody}");

        // Act - Login
        var loginRequest = new
        {
            Email = email,
            Password = "Password123"
        };

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert - Login
        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);

        var loginContent =
            await loginResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        loginContent
            .GetProperty("userId")
            .GetGuid()
            .Should()
            .NotBe(Guid.Empty);

        loginContent
            .GetProperty("accessToken")
            .GetString()
            .Should()
            .NotBeNullOrWhiteSpace();

        loginContent
            .GetProperty("refreshToken")
            .GetString()
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldLockAccount_AfterFiveFailedAttempts()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"lockout-{Guid.NewGuid():N}@example.com";

        const string correctPassword =
            "Password123";

        const string wrongPassword =
            "WrongPassword123";

        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    FirstName = "İsmail",
                    LastName = "Karasu",
                    Email = email,
                    Password = correctPassword,
                    ConfirmPassword = correctPassword
                });

        var registerBody =
            await registerResponse.Content
                .ReadAsStringAsync();

        registerResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"register response: {registerBody}");

        // Act - Beş defa yanlış parola gönder
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var failedLoginResponse =
                await client.PostAsJsonAsync(
                    "/api/auth/login",
                    new
                    {
                        Email = email,
                        Password = wrongPassword
                    });

            var failedLoginBody =
                await failedLoginResponse.Content
                    .ReadAsStringAsync();

            // Assert - Her yanlış deneme reddedilir
            failedLoginResponse.StatusCode.Should().Be(
                HttpStatusCode.Unauthorized,
                $"failed login attempt {attempt}: " +
                failedLoginBody);

            using var failedLoginJson =
                JsonDocument.Parse(failedLoginBody);

            failedLoginJson.RootElement
                .GetProperty("code")
                .GetString()
                .Should()
                .Be(
                    "Authentication.InvalidCredentials");
        }

        // Act - Doğru parola ile giriş yapmayı dene
        var lockedLoginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    Email = email,
                    Password = correctPassword
                });

        var lockedLoginBody =
            await lockedLoginResponse.Content
                .ReadAsStringAsync();

        // Assert - Doğru parola olsa bile hesap kilitli
        lockedLoginResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized,
            $"locked login response: {lockedLoginBody}");

        using var lockedLoginJson =
            JsonDocument.Parse(lockedLoginBody);

        lockedLoginJson.RootElement
            .GetProperty("code")
            .GetString()
            .Should()
            .Be(
                "Authentication.InvalidCredentials");
    }

    [Fact]
    public async Task Refresh_ShouldRotateToken_AndRejectOldToken()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"refresh-{Guid.NewGuid():N}@example.com";

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

        var oldRefreshToken =
            loginJson.RootElement
                .GetProperty("refreshToken")
                .GetString();

        oldRefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        // Act - İlk refresh
        var refreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = oldRefreshToken
                });

        var refreshBody =
            await refreshResponse.Content
                .ReadAsStringAsync();

        // Assert - Yeni token üretildi
        refreshResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"refresh response: {refreshBody}");

        using var refreshJson =
            JsonDocument.Parse(refreshBody);

        var newRefreshToken =
            refreshJson.RootElement
                .GetProperty("refreshToken")
                .GetString();

        newRefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        newRefreshToken.Should()
            .NotBe(oldRefreshToken);

        // Act - Eski token tekrar kullanılıyor
        var reuseResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = oldRefreshToken
                });

        // Assert
        reuseResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken()
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"logout-{Guid.NewGuid():N}@example.com";

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

        var refreshToken =
            loginJson.RootElement
                .GetProperty("refreshToken")
                .GetString();

        refreshToken.Should()
            .NotBeNullOrWhiteSpace();

        // Act - Logout
        var logoutResponse =
            await client.PostAsJsonAsync(
                "/api/auth/logout",
                new
                {
                    RefreshToken = refreshToken
                });

        // Assert - Logout başarılı
        logoutResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act - İptal edilen token tekrar kullanılıyor
        var refreshResponse =
            await client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = refreshToken
                });

        // Assert
        refreshResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("en", "Email or password is incorrect.")]
    [InlineData("tr", "E-posta adresi veya şifre hatalı.")]
    [InlineData("de", "E-Mail-Adresse oder Passwort ist falsch.")]
    public async Task Login_ShouldReturnLocalizedError_WhenCredentialsAreInvalid(
    string language,
    string expectedMessage)
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        client.DefaultRequestHeaders
            .AcceptLanguage
            .ParseAdd(language);

        var request = new
        {
            Email =
                $"missing-{Guid.NewGuid():N}@example.com",
            Password = "WrongPassword123"
        };

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        var responseBody =
            await response.Content
                .ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized,
            $"response body: {responseBody}");

        using var responseJson =
            JsonDocument.Parse(responseBody);

        var error = responseJson.RootElement;

        error.GetProperty("code")
            .GetString()
            .Should()
            .Be(
                "Authentication.InvalidCredentials");

        error.GetProperty("message")
            .GetString()
            .Should()
            .Be(expectedMessage);
    }
}