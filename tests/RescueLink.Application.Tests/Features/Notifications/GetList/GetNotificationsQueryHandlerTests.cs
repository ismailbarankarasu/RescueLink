using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
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

    [Fact]
    public async Task Handle_ShouldReturnNotifications_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        var query = new GetNotificationsQuery(
            Page: 2,
            PageSize: 10,
            UnreadOnly: true);

        var item = new NotificationListItemResponse(
            Id: Guid.NewGuid(),
            Type: NotificationType.MatchSuggested,
            Title: "Yeni eşleşme önerisi",
            Message:
                "İlanınız için yeni bir eşleşme bulundu.",
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
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _notificationReadServiceMock
            .Setup(x => x.GetAsync(
                userId,
                query.Page,
                query.PageSize,
                query.UnreadOnly,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var handler = CreateHandler();

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pagedResult);

        _notificationReadServiceMock.Verify(
            x => x.GetAsync(
                userId,
                2,
                10,
                true,
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
            new GetNotificationsQuery(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            NotificationErrors.Unauthenticated.Code);

        _notificationReadServiceMock.Verify(
            x => x.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetNotificationsQueryHandler CreateHandler()
    {
        return new GetNotificationsQueryHandler(
            _notificationReadServiceMock.Object,
            _currentUserServiceMock.Object);
    }
}