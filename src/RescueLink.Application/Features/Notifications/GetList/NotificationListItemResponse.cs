using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.Notifications.GetList;

public sealed record NotificationListItemResponse(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);