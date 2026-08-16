using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.Notifications;
using RescueLink.Application.Features.Notifications.MarkAsRead;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests
    .Features.Notifications.MarkAsRead;

public sealed class MarkNotificationAsReadCommandHandlerTests
{
    private readonly Mock<IUserNotificationRepository>
        _notificationRepositoryMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ShouldMarkAsRead_WhenUserOwnsNotification()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                notification.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkNotificationAsReadCommand(
                notification.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkNotificationAsReadCommand(
                Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors.Unauthenticated.Code);

        _notificationRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNotificationDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                notificationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserNotification?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkNotificationAsReadCommand(
                notificationId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors
                .NotFound(notificationId)
                .Code);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotOwnNotification()
    {
        var ownerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var notification =
            CreateNotification(ownerId);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(currentUserId);

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                notification.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkNotificationAsReadCommand(
                notification.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors.Forbidden.Code);

        notification.IsRead.Should().BeFalse();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotSave_WhenNotificationIsAlreadyRead()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);

        notification.MarkAsRead();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                notification.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkNotificationAsReadCommand(
                notification.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private MarkNotificationAsReadCommandHandler
        CreateHandler()
    {
        return new MarkNotificationAsReadCommandHandler(
            _notificationRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    private static UserNotification CreateNotification(
        Guid userId)
    {
        return UserNotification.Create(
            userId: userId,
            type: NotificationType.MatchSuggested,
            title: "Yeni eşleşme önerisi",
            message:
                "İlanınız için yeni bir eşleşme bulundu.",
            relatedEntityId: Guid.NewGuid());
    }
}