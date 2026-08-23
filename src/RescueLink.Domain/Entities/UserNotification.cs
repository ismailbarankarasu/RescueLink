using RescueLink.Domain.Common;
using RescueLink.Domain.Enums;

namespace RescueLink.Domain.Entities;

public sealed class UserNotification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }

    public string Title { get; private set; } =
        string.Empty;

    public string Message { get; private set; } =
        string.Empty;

    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private UserNotification()
    {
    }

    public static UserNotification Create(Guid userId,NotificationType type,string title,string message, Guid? relatedEntityId = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                "Notification type is invalid.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            title);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            message);

        return new UserNotification
        {
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            ReadAt = null
        };
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}