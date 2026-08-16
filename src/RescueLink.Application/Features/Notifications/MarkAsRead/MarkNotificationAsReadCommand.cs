using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications.MarkAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid NotificationId)
    : ICommand<Result>;