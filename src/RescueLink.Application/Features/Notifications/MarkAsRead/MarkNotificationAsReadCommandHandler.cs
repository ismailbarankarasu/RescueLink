using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications.MarkAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<
        MarkNotificationAsReadCommand,
        Result>
{
    private readonly IUserNotificationRepository
        _notificationRepository;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IUnitOfWork
        _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        IUserNotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(
                NotificationErrors.Unauthenticated);
        }

        var notification =
            await _notificationRepository.GetByIdAsync(
                request.NotificationId,
                cancellationToken);

        if (notification is null)
        {
            return Result.Failure(
                NotificationErrors.NotFound(
                    request.NotificationId));
        }

        if (notification.UserId !=
            _currentUserService.UserId.Value)
        {
            return Result.Failure(
                NotificationErrors.Forbidden);
        }

        if (notification.IsRead)
        {
            return Result.Success();
        }

        notification.MarkAsRead();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}