using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Localization;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application
    .Features.Notifications.GetList;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationListItemResponse>>>
{
    private readonly INotificationReadService
        _notificationReadService;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly INotificationContentLocalizer
        _notificationContentLocalizer;

    public GetNotificationsQueryHandler(INotificationReadService notificationReadService, ICurrentUserService currentUserService, INotificationContentLocalizer notificationContentLocalizer)
    {
        _notificationReadService = notificationReadService;
        _currentUserService = currentUserService;
        _notificationContentLocalizer = notificationContentLocalizer;
    }

    public async Task<
        Result<PagedResult<
            NotificationListItemResponse>>> Handle(
                GetNotificationsQuery request,
                CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                PagedResult<
                    NotificationListItemResponse>>(
                        NotificationErrors
                            .Unauthenticated);
        }

        var notifications =
            await _notificationReadService.GetAsync(
                userId:
                    _currentUserService.UserId.Value,
                page: request.Page,
                pageSize: request.PageSize,
                unreadOnly: request.UnreadOnly,
                cancellationToken:
                    cancellationToken);

        var localizedItems =
            notifications.Items
                .Select(notification =>
                {
                    var content =
                        _notificationContentLocalizer
                            .Localize(
                                notification.Type,
                                notification.Title,
                                notification.Message);

                    return notification with
                    {
                        Title = content.Title,
                        Message = content.Message
                    };
                })
                .ToArray();

        var localizedNotifications =
            new PagedResult<
                NotificationListItemResponse>(
                    Items: localizedItems,
                    Page: notifications.Page,
                    PageSize: notifications.PageSize,
                    TotalCount:
                        notifications.TotalCount);

        return Result.Success(
            localizedNotifications);
    }
}