using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications.GetList;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<
        GetNotificationsQuery,
        Result<PagedResult<NotificationListItemResponse>>>
{
    private readonly INotificationReadService
        _notificationReadService;

    private readonly ICurrentUserService
        _currentUserService;

    public GetNotificationsQueryHandler(
        INotificationReadService notificationReadService,
        ICurrentUserService currentUserService)
    {
        _notificationReadService = notificationReadService;
        _currentUserService = currentUserService;
    }

    public async Task<
        Result<PagedResult<NotificationListItemResponse>>> Handle(
            GetNotificationsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                PagedResult<NotificationListItemResponse>>(
                    NotificationErrors.Unauthenticated);
        }

        var notifications =
            await _notificationReadService.GetAsync(
                userId: _currentUserService.UserId.Value,
                page: request.Page,
                pageSize: request.PageSize,
                unreadOnly: request.UnreadOnly,
                cancellationToken: cancellationToken);

        return Result.Success(notifications);
    }
}