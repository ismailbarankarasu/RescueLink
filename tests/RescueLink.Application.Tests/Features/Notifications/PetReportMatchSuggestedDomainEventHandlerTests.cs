using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Features.Notifications;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests
    .Features.Notifications;

public sealed class
    PetReportMatchSuggestedDomainEventHandlerTests
{
    private readonly Mock<IPetReportRepository>
        _petReportRepositoryMock = new();

    private readonly Mock<IUserNotificationRepository>
        _notificationRepositoryMock = new();

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ShouldCreateNotifications_ForBothOwners()
    {
        var lostOwnerId = Guid.NewGuid();
        var foundOwnerId = Guid.NewGuid();

        var lostReport = CreateReport(
            lostOwnerId,
            ReportType.Lost);

        var foundReport = CreateReport(
            foundOwnerId,
            ReportType.Found);

        var matchId = Guid.NewGuid();

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport, foundReport]);

        IReadOnlyCollection<UserNotification>?
            capturedNotifications = null;

        _notificationRepositoryMock
            .Setup(x => x.AddRangeAsync(
                It.IsAny<
                    IReadOnlyCollection<UserNotification>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                IReadOnlyCollection<UserNotification>,
                CancellationToken>(
                (notifications, _) =>
                    capturedNotifications = notifications)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var handler = CreateHandler();

        var notification = CreateNotification(
            matchId,
            lostReport.Id,
            foundReport.Id);

        await handler.Handle(
            notification,
            CancellationToken.None);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(2);

        capturedNotifications!
            .Select(item => item.UserId)
            .Should()
            .BeEquivalentTo(
                [lostOwnerId, foundOwnerId]);

        capturedNotifications.Should().OnlyContain(
            item =>
                item.Type ==
                    NotificationType.MatchSuggested &&
                item.RelatedEntityId == matchId &&
                !item.IsRead);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateNotification_WhenReportIsMissing()
    {
        var lostReport = CreateReport(
            Guid.NewGuid(),
            ReportType.Lost);

        var missingFoundReportId =
            Guid.NewGuid();

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport]);

        var handler = CreateHandler();

        var notification = CreateNotification(
            matchId: Guid.NewGuid(),
            lostReportId: lostReport.Id,
            foundReportId: missingFoundReportId);

        await handler.Handle(
            notification,
            CancellationToken.None);

        _notificationRepositoryMock.Verify(
            x => x.AddRangeAsync(
                It.IsAny<
                    IReadOnlyCollection<UserNotification>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PetReportMatchSuggestedDomainEventHandler
        CreateHandler()
    {
        return new PetReportMatchSuggestedDomainEventHandler(
            _petReportRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static DomainEventNotification<
        PetReportMatchSuggestedDomainEvent>
        CreateNotification(
            Guid matchId,
            Guid lostReportId,
            Guid foundReportId)
    {
        var domainEvent =
            new PetReportMatchSuggestedDomainEvent(
                MatchId: matchId,
                LostReportId: lostReportId,
                FoundReportId: foundReportId);

        return new DomainEventNotification<
            PetReportMatchSuggestedDomainEvent>(
                domainEvent);
    }

    private static PetReport CreateReport(
        Guid userId,
        ReportType reportType)
    {
        return PetReport.Create(
            userId: userId,
            reportType: reportType,
            title: "Golden ilanı",
            description: "Golden Retriever ilanı.",
            species: AnimalSpecies.Dog,
            gender: AnimalGender.Female,
            petName: null,
            breed: "Golden Retriever",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.2000,
                longitude: 29.0300));
    }
}