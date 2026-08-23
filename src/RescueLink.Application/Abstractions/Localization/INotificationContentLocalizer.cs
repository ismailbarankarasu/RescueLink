using RescueLink.Domain.Enums;

namespace RescueLink.Application
    .Abstractions.Localization;

public interface INotificationContentLocalizer
{
    NotificationContent Localize(
        NotificationType type,
        string fallbackTitle,
        string fallbackMessage);
}