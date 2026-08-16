using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications.GetList;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    bool UnreadOnly = false)
    : IQuery<Result<
        PagedResult<NotificationListItemResponse>>>;