using System.Globalization;
using FluentAssertions;
using RescueLink.Application.Features.Notifications;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests
    .Features.Notifications;

public sealed class NotificationContentLocalizerTests
{
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
    public void Localize_ShouldReturnRequestedLanguage(
        string cultureName,
        string expectedTitle,
        string expectedMessage)
    {
        // Arrange
        var originalCulture =
            CultureInfo.CurrentCulture;

        var originalUiCulture =
            CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture =
                new CultureInfo(cultureName);

            CultureInfo.CurrentUICulture =
                new CultureInfo(cultureName);

            var localizer =
                new NotificationContentLocalizer();

            // Act
            var result = localizer.Localize(
                NotificationType.MatchSuggested,
                fallbackTitle: "Fallback title",
                fallbackMessage: "Fallback message");

            // Assert
            result.Title.Should().Be(
                expectedTitle);

            result.Message.Should().Be(
                expectedMessage);
        }
        finally
        {
            CultureInfo.CurrentCulture =
                originalCulture;

            CultureInfo.CurrentUICulture =
                originalUiCulture;
        }
    }

    [Fact]
    public void Localize_ShouldReturnFallback_WhenTypeIsUnknown()
    {
        // Arrange
        var localizer =
            new NotificationContentLocalizer();

        const string fallbackTitle =
            "Stored title";

        const string fallbackMessage =
            "Stored message";

        // Act
        var result = localizer.Localize(
            type: (NotificationType)999,
            fallbackTitle: fallbackTitle,
            fallbackMessage: fallbackMessage);

        // Assert
        result.Title.Should().Be(
            fallbackTitle);

        result.Message.Should().Be(
            fallbackMessage);
    }
}