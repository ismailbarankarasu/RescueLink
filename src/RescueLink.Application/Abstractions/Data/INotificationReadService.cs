using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.Notifications.GetList;

namespace RescueLink.Application.Abstractions.Data;

public interface INotificationReadService
{
    Task<PagedResult<NotificationListItemResponse>> GetAsync(
        Guid userId,
        int page,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}