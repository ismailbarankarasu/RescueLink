using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.Notifications;
using RescueLink.Application.Features.Notifications.MarkAllAsRead;

namespace RescueLink.Application.Tests
    .Features.Notifications.MarkAllAsRead;

public sealed class
    MarkAllNotificationsAsReadCommandHandlerTests
{
    private readonly Mock<IUserNotificationRepository>
        _notificationRepositoryMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldMarkAllAsRead_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationRepositoryMock
            .Setup(x => x.MarkAllAsReadAsync(
                userId,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkAllNotificationsAsReadCommand(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3);

        _notificationRepositoryMock.Verify(
            x => x.MarkAllAsReadAsync(
                userId,
                It.Is<DateTimeOffset>(
                    date => date <= DateTimeOffset.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenNoUnreadNotificationExists()
    {
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationRepositoryMock
            .Setup(x => x.MarkAllAsReadAsync(
                userId,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkAllNotificationsAsReadCommand(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new MarkAllNotificationsAsReadCommand(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors.Unauthenticated.Code);

        _notificationRepositoryMock.Verify(
            x => x.MarkAllAsReadAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private MarkAllNotificationsAsReadCommandHandler
        CreateHandler()
    {
        return new MarkAllNotificationsAsReadCommandHandler(
            _notificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }
}