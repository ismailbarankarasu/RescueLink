using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications
    .MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<
        MarkAllNotificationsAsReadCommand,
        Result<int>>
{
    private readonly IUserNotificationRepository
        _notificationRepository;

    private readonly ICurrentUserService
        _currentUserService;

    public MarkAllNotificationsAsReadCommandHandler(
        IUserNotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<int>(
                NotificationErrors.Unauthenticated);
        }

        var updatedCount =
            await _notificationRepository
                .MarkAllAsReadAsync(
                    userId:
                        _currentUserService.UserId.Value,
                    readAt: DateTimeOffset.UtcNow,
                    cancellationToken:
                        cancellationToken);

        return Result.Success(updatedCount);
    }
}