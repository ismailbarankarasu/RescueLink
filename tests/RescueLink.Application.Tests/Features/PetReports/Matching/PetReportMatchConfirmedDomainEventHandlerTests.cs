using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Features.PetReports.Matching;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features.PetReports.Matching;

public sealed class PetReportMatchConfirmedDomainEventHandlerTests
{
    private readonly Mock<IPetReportRepository>
        _petReportRepositoryMock = new();

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ShouldResolveBothReports_WhenBothAreActive()
    {
        var lostReport = CreateReport(ReportType.Lost);
        var foundReport = CreateReport(ReportType.Found);

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport, foundReport]);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var handler = CreateHandler();

        var notification = CreateNotification(
            lostReport.Id,
            foundReport.Id);

        await handler.Handle(
            notification,
            CancellationToken.None);

        lostReport.Status.Should().Be(ReportStatus.Resolved);
        foundReport.Status.Should().Be(ReportStatus.Resolved);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldResolveOnlyActiveReport()
    {
        var lostReport = CreateReport(ReportType.Lost);
        var foundReport = CreateReport(ReportType.Found);

        lostReport.Cancel();

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport, foundReport]);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        var notification = CreateNotification(
            lostReport.Id,
            foundReport.Id);

        await handler.Handle(
            notification,
            CancellationToken.None);

        lostReport.Status.Should().Be(ReportStatus.Cancelled);
        foundReport.Status.Should().Be(ReportStatus.Resolved);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSave_WhenNoReportIsActive()
    {
        var lostReport = CreateReport(ReportType.Lost);
        var foundReport = CreateReport(ReportType.Found);

        lostReport.Resolve();
        foundReport.Cancel();

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport, foundReport]);

        var handler = CreateHandler();

        var notification = CreateNotification(
            lostReport.Id,
            foundReport.Id);

        await handler.Handle(
            notification,
            CancellationToken.None);

        lostReport.Status.Should().Be(ReportStatus.Resolved);
        foundReport.Status.Should().Be(ReportStatus.Cancelled);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PetReportMatchConfirmedDomainEventHandler CreateHandler()
    {
        return new PetReportMatchConfirmedDomainEventHandler(
            _petReportRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static DomainEventNotification<
        PetReportMatchConfirmedDomainEvent> CreateNotification(
            Guid lostReportId,
            Guid foundReportId)
    {
        var domainEvent =
            new PetReportMatchConfirmedDomainEvent(
                MatchId: Guid.NewGuid(),
                LostReportId: lostReportId,
                FoundReportId: foundReportId);

        return new DomainEventNotification<
            PetReportMatchConfirmedDomainEvent>(
                domainEvent);
    }

    private static PetReport CreateReport(
        ReportType reportType)
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: reportType,
            title: "Tekir kedi ilanı",
            description: "Gri ve beyaz tekir kedi.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Male,
            petName: null,
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.2165,
                longitude: 28.9849));
    }
}