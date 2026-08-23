using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RescueLink.API.IntegrationTests.Infrastructure;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Persistence.Context;

namespace RescueLink.API.IntegrationTests
    .Features.Notifications;

public sealed class NotificationLocalizationEndpointTests
    : IClassFixture<SqlServerContainerFixture>
{
    private readonly SqlServerContainerFixture
        _sqlServerContainer;

    public NotificationLocalizationEndpointTests(
        SqlServerContainerFixture sqlServerContainer)
    {
        _sqlServerContainer = sqlServerContainer;
    }

    [Theory]
    [InlineData(
        "en",
        "New match suggestion",
        "A potential match was found for your pet report.")]
    [InlineData(
        "tr",
        "Yeni eşleşme önerisi",
        "Hayvan ilanınız için olası bir eşleşme bulundu.")]
    [InlineData(
        "de",
        "Neuer Übereinstimmungsvorschlag",
        "Für Ihre Tiermeldung wurde eine mögliche Übereinstimmung gefunden.")]
    public async Task GetNotifications_ShouldReturnLocalizedContent(
        string language,
        string expectedTitle,
        string expectedMessage)
    {
        // Arrange
        await using var factory =
            new RescueLinkWebApplicationFactory(
                _sqlServerContainer.ConnectionString);

        using var client = factory.CreateClient();

        var email =
            $"notification-{Guid.NewGuid():N}@example.com";

        const string password = "Password123";

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

        using var registerJson =
            JsonDocument.Parse(registerBody);

        var userId =
            registerJson.RootElement
                .GetProperty("userId")
                .GetGuid();

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

        await using (var scope =
            factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        RescueLinkDbContext>();

            var notification =
                UserNotification.Create(
                    userId: userId,
                    type:
                        NotificationType.MatchSuggested,
                    title: "Stored fallback title",
                    message: "Stored fallback message",
                    relatedEntityId: Guid.NewGuid());

            await dbContext.UserNotifications.AddAsync(
                notification);

            await dbContext.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        client.DefaultRequestHeaders
            .AcceptLanguage
            .Clear();

        client.DefaultRequestHeaders
            .AcceptLanguage
            .Add(
                new StringWithQualityHeaderValue(
                    language));

        // Act
        var response =
            await client.GetAsync(
                "/api/notifications" +
                "?page=1" +
                "&pageSize=20" +
                "&unreadOnly=false");

        var responseBody =
            await response.Content
                .ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"notifications response: {responseBody}");

        using var responseJson =
            JsonDocument.Parse(responseBody);

        var notificationItem =
            responseJson.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Single(item =>
                    item.GetProperty("type")
                        .GetString() ==
                    "MatchSuggested");

        notificationItem
            .GetProperty("title")
            .GetString()
            .Should()
            .Be(expectedTitle);

        notificationItem
            .GetProperty("message")
            .GetString()
            .Should()
            .Be(expectedMessage);
    }
}