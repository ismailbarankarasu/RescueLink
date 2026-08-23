using RescueLink.Application.Abstractions.Localization;
using RescueLink.Application.Localization;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.Notifications;
public sealed class NotificationContentLocalizer : INotificationContentLocalizer
{
    public NotificationContent Localize(NotificationType type, string fallbackTitle, string fallbackMessage)
    {
        return type switch
        {
            NotificationType.MatchSuggested =>
                new NotificationContent(
                    NotificationMessages
                        .MatchSuggestedTitle,
                    NotificationMessages
                        .MatchSuggestedMessage),

            NotificationType.MatchConfirmed =>
                new NotificationContent(
                    NotificationMessages
                        .MatchConfirmedTitle,
                    NotificationMessages
                        .MatchConfirmedMessage),

            NotificationType.ReportResolved =>
                new NotificationContent(
                    NotificationMessages
                        .ReportResolvedTitle,
                    NotificationMessages
                        .ReportResolvedMessage),

            _ => new NotificationContent(
                fallbackTitle,
                fallbackMessage)
        };
    }
}