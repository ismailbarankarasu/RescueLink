using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

namespace RescueLink.Domain.Tests.Entities;

public sealed class UserNotificationTests
{
    [Fact]
    public void Create_ShouldCreateUnreadNotification()
    {
        var userId = Guid.NewGuid();
        var relatedEntityId = Guid.NewGuid();

        var notification = UserNotification.Create(
            userId: userId,
            type: NotificationType.MatchSuggested,
            title: "  Yeni eşleşme bulundu  ",
            message: "  İlanınız için uygun bir eşleşme var.  ",
            relatedEntityId: relatedEntityId);

        notification.UserId.Should().Be(userId);

        notification.Type.Should().Be(
            NotificationType.MatchSuggested);

        notification.Title.Should().Be(
            "Yeni eşleşme bulundu");

        notification.Message.Should().Be(
            "İlanınız için uygun bir eşleşme var.");

        notification.RelatedEntityId.Should()
            .Be(relatedEntityId);

        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var action = () => UserNotification.Create(
            userId: Guid.Empty,
            type: NotificationType.MatchSuggested,
            title: "Yeni eşleşme",
            message: "Yeni bir eşleşme bulundu.");

        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("userId");
    }

    [Fact]
    public void Create_ShouldThrow_WhenTypeIsInvalid()
    {
        var action = () => UserNotification.Create(
            userId: Guid.NewGuid(),
            type: (NotificationType)999,
            title: "Yeni eşleşme",
            message: "Yeni bir eşleşme bulundu.");

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("type");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenTitleIsEmpty(
        string title)
    {
        var action = () => UserNotification.Create(
            userId: Guid.NewGuid(),
            type: NotificationType.MatchSuggested,
            title: title,
            message: "Yeni bir eşleşme bulundu.");

        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenMessageIsEmpty(
        string message)
    {
        var action = () => UserNotification.Create(
            userId: Guid.NewGuid(),
            type: NotificationType.MatchSuggested,
            title: "Yeni eşleşme",
            message: message);

        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("message");
    }

    [Fact]
    public void MarkAsRead_ShouldMarkNotificationAsRead()
    {
        var notification = CreateNotification();

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsRead_ShouldBeIdempotent()
    {
        var notification = CreateNotification();

        notification.MarkAsRead();

        var firstReadAt = notification.ReadAt;
        var firstUpdatedAt = notification.UpdatedAt;

        notification.MarkAsRead();

        notification.ReadAt.Should().Be(firstReadAt);
        notification.UpdatedAt.Should().Be(firstUpdatedAt);
    }

    private static UserNotification CreateNotification()
    {
        return UserNotification.Create(
            userId: Guid.NewGuid(),
            type: NotificationType.MatchSuggested,
            title: "Yeni eşleşme bulundu",
            message: "İlanınız için uygun bir eşleşme var.",
            relatedEntityId: Guid.NewGuid());
    }
}