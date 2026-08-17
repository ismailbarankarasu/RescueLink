using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Features.Notifications;
using RescueLink.Application.Features.Notifications.GetUnreadCount;

namespace RescueLink.Application.Tests
    .Features.Notifications.GetUnreadCount;

public sealed class
    GetUnreadNotificationCountQueryHandlerTests
{
    private readonly Mock<INotificationReadService>
        _notificationReadServiceMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnUnreadCount_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationReadServiceMock
            .Setup(x => x.GetUnreadCountAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new GetUnreadNotificationCountQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3);

        _notificationReadServiceMock.Verify(
            x => x.GetUnreadCountAsync(
                userId,
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
            new GetUnreadNotificationCountQuery(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors.Unauthenticated.Code);

        _notificationReadServiceMock.Verify(
            x => x.GetUnreadCountAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetUnreadNotificationCountQueryHandler
        CreateHandler()
    {
        return new GetUnreadNotificationCountQueryHandler(
            _notificationReadServiceMock.Object,
            _currentUserServiceMock.Object);
    }
}