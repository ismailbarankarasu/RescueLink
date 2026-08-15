using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReportMatches.Confirm;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests
    .Features.PetReportMatches.Confirm;

public sealed class ConfirmPetReportMatchCommandHandlerTests
{
    private readonly Mock<IPetReportMatchRepository>
        _matchRepositoryMock = new();

    private readonly Mock<IPetReportRepository>
        _petReportRepositoryMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ShouldConfirmMatch_WhenBothOwnersConfirm()
    {
        var lostOwnerId = Guid.NewGuid();
        var foundOwnerId = Guid.NewGuid();

        var lostReport = CreateReport(
            lostOwnerId,
            ReportType.Lost);

        var foundReport = CreateReport(
            foundOwnerId,
            ReportType.Found);

        var match = PetReportMatch.Create(
            lostReport.Id,
            foundReport.Id,
            score: 100,
            distanceMeters: 61);

        Guid? currentUserId = lostOwnerId;

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(() => currentUserId);

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(
                match.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport, foundReport]);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new ConfirmPetReportMatchCommand(match.Id);

        var firstResult = await handler.Handle(
            command,
            CancellationToken.None);

        firstResult.IsSuccess.Should().BeTrue();
        match.LostOwnerConfirmed.Should().BeTrue();
        match.FoundOwnerConfirmed.Should().BeFalse();
        match.Status.Should().Be(MatchStatus.Suggested);

        currentUserId = foundOwnerId;

        var secondResult = await handler.Handle(
            command,
            CancellationToken.None);

        secondResult.IsSuccess.Should().BeTrue();
        match.LostOwnerConfirmed.Should().BeTrue();
        match.FoundOwnerConfirmed.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Confirmed);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ConfirmPetReportMatchCommand(
                Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _matchRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserOwnsNeitherReport()
    {
        var lostReport = CreateReport(
            Guid.NewGuid(),
            ReportType.Lost);

        var foundReport = CreateReport(
            Guid.NewGuid(),
            ReportType.Found);

        var match = PetReportMatch.Create(
            lostReport.Id,
            foundReport.Id,
            score: 90,
            distanceMeters: 500);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(Guid.NewGuid());

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(
                match.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        _petReportRepositoryMock
            .Setup(x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([lostReport, foundReport]);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ConfirmPetReportMatchCommand(match.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        match.LostOwnerConfirmed.Should().BeFalse();
        match.FoundOwnerConfirmed.Should().BeFalse();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMatchIsNotSuggested()
    {
        var lostOwnerId = Guid.NewGuid();
        var foundOwnerId = Guid.NewGuid();

        var lostReport = CreateReport(
            lostOwnerId,
            ReportType.Lost);

        var foundReport = CreateReport(
            foundOwnerId,
            ReportType.Found);

        var match = PetReportMatch.Create(
            lostReport.Id,
            foundReport.Id,
            score: 100,
            distanceMeters: 100);

        match.Confirm(lostReport.Id);
        match.Confirm(foundReport.Id);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(lostOwnerId);

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(
                match.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ConfirmPetReportMatchCommand(match.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        match.Status.Should().Be(MatchStatus.Confirmed);

        _petReportRepositoryMock.Verify(
            x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ConfirmPetReportMatchCommandHandler CreateHandler()
    {
        return new ConfirmPetReportMatchCommandHandler(
            _matchRepositoryMock.Object,
            _petReportRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    private static PetReport CreateReport(
        Guid userId,
        ReportType reportType)
    {
        return PetReport.Create(
            userId: userId,
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