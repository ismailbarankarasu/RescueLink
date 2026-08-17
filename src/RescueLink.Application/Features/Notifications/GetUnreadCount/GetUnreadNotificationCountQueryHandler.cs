using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications
    .GetUnreadCount;

public sealed class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<
        GetUnreadNotificationCountQuery,
        Result<int>>
{
    private readonly INotificationReadService
        _notificationReadService;

    private readonly ICurrentUserService
        _currentUserService;

    public GetUnreadNotificationCountQueryHandler(
        INotificationReadService notificationReadService,
        ICurrentUserService currentUserService)
    {
        _notificationReadService = notificationReadService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<int>(
                NotificationErrors.Unauthenticated);
        }

        var count =
            await _notificationReadService
                .GetUnreadCountAsync(
                    _currentUserService.UserId.Value,
                    cancellationToken);

        return Result.Success(count);
    }
}