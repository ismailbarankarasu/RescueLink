using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Localization;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.Notifications;
using RescueLink.Application.Features.Notifications.GetList;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests
    .Features.Notifications.GetList;

public sealed class GetNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationReadService>
        _notificationReadServiceMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    private readonly Mock<INotificationContentLocalizer>
        _notificationContentLocalizerMock = new();

    [Fact]
    public async Task Handle_ShouldReturnLocalizedNotifications_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var query = new GetNotificationsQuery(
            Page: 2,
            PageSize: 10,
            UnreadOnly: true);

        var item = new NotificationListItemResponse(
            Id: Guid.NewGuid(),
            Type: NotificationType.MatchSuggested,
            Title: "Stored title",
            Message: "Stored message",
            RelatedEntityId: Guid.NewGuid(),
            IsRead: false,
            ReadAt: null,
            CreatedAt: DateTimeOffset.UtcNow);

        var pagedResult =
            new PagedResult<NotificationListItemResponse>(
                Items: [item],
                Page: 2,
                PageSize: 10,
                TotalCount: 11);

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _notificationReadServiceMock
            .Setup(service => service.GetAsync(
                userId,
                query.Page,
                query.PageSize,
                query.UnreadOnly,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _notificationContentLocalizerMock
            .Setup(localizer => localizer.Localize(
                NotificationType.MatchSuggested,
                "Stored title",
                "Stored message"))
            .Returns(
                new NotificationContent(
                    Title: "Localized title",
                    Message: "Localized message"));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(11);

        var resultItem =
            result.Value.Items.Should()
                .ContainSingle()
                .Subject;

        resultItem.Id.Should().Be(item.Id);

        resultItem.Type.Should().Be(
            NotificationType.MatchSuggested);

        resultItem.Title.Should().Be(
            "Localized title");

        resultItem.Message.Should().Be(
            "Localized message");

        resultItem.RelatedEntityId.Should().Be(
            item.RelatedEntityId);

        resultItem.IsRead.Should().BeFalse();

        _notificationReadServiceMock.Verify(
            service => service.GetAsync(
                userId,
                2,
                10,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationContentLocalizerMock.Verify(
            localizer => localizer.Localize(
                NotificationType.MatchSuggested,
                "Stored title",
                "Stored message"),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        // Arrange
        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetNotificationsQuery(),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors
                .Unauthenticated.Code);

        _notificationReadServiceMock.Verify(
            service => service.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationContentLocalizerMock.Verify(
            localizer => localizer.Localize(
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    private GetNotificationsQueryHandler
        CreateHandler()
    {
        return new GetNotificationsQueryHandler(
            _notificationReadServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationContentLocalizerMock.Object);
    }
}