using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Context;

namespace RescueLink.Persistence.Repositories;

internal sealed class UserNotificationRepository(
    RescueLinkDbContext dbContext)
    : IUserNotificationRepository
{
    public async Task AddRangeAsync(
        IReadOnlyCollection<UserNotification> notifications,
        CancellationToken cancellationToken = default)
    {
        if (notifications.Count == 0)
        {
            return;
        }

        await dbContext.UserNotifications.AddRangeAsync(
            notifications,
            cancellationToken);
    }

    public async Task<UserNotification?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserNotifications
            .SingleOrDefaultAsync(
                notification =>
                    notification.Id == notificationId,
                cancellationToken);
    }
}