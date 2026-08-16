using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications;

public static class NotificationErrors
{
    public static readonly Error Unauthenticated = new(
        "Authentication.Unauthenticated",
        "The current user is not authenticated.");

    public static Error NotFound(Guid notificationId)
    {
        return new Error(
            "Notification.NotFound",
            $"Notification '{notificationId}' was not found.");
    }

    public static readonly Error Forbidden = new(
        "Notification.Forbidden",
        "You are not allowed to manage this notification.");
}