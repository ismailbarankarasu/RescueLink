using RescueLink.Domain.Entities;

namespace RescueLink.Application.Abstractions.Persistence;

public interface IUserNotificationRepository
{
    Task AddRangeAsync(
        IReadOnlyCollection<UserNotification> notifications,
        CancellationToken cancellationToken = default);

    Task<UserNotification?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        Guid userId,
        DateTimeOffset readAt,
        CancellationToken cancellationToken = default);
}