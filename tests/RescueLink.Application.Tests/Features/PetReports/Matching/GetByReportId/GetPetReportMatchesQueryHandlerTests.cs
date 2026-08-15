using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features.PetReports
    .Matching.GetByReportId;

public sealed class GetPetReportMatchesQueryHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock =
        new();

    private readonly Mock<IPetReportMatchReadService>
        _matchReadServiceMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnMatches_WhenUserOwnsReport()
    {
        var userId = Guid.NewGuid();
        var report = CreatePetReport(userId);

        IReadOnlyCollection<PetReportMatchResponse> matches =
        [
            new PetReportMatchResponse(
                MatchId: Guid.NewGuid(),
                CounterpartReportId: Guid.NewGuid(),
                ReportType: ReportType.Found,
                Title: "Bulunan tekir kedi",
                Species: AnimalSpecies.Cat,
                Gender: AnimalGender.Male,
                Breed: "Tekir",
                PrimaryColor: AnimalColor.Gray,
                SecondaryColor: AnimalColor.White,
                EventDate: DateTimeOffset.UtcNow.AddHours(-1),
                Latitude: 40.217,
                Longitude: 28.9852,
                Score: 100,
                DistanceMeters: 61,
                Status: MatchStatus.Suggested,
                PrimaryPhotoStorageKey: null)
        ];

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdReadOnlyAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        _matchReadServiceMock
            .Setup(x => x.GetByReportIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        var handler = CreateHandler();
        var query = new GetPetReportMatchesQuery(report.Id);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(matches);

        _matchReadServiceMock.Verify(
            x => x.GetByReportIdAsync(
                report.Id,
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
            new GetPetReportMatchesQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdReadOnlyAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _matchReadServiceMock.Verify(
            x => x.GetByReportIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReportDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdReadOnlyAsync(
                reportId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetReport?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new GetPetReportMatchesQuery(reportId),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _matchReadServiceMock.Verify(
            x => x.GetByReportIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReportBelongsToAnotherUser()
    {
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var report = CreatePetReport(ownerUserId);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(currentUserId);

        _repositoryMock
            .Setup(x => x.GetByIdReadOnlyAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new GetPetReportMatchesQuery(report.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _matchReadServiceMock.Verify(
            x => x.GetByReportIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetPetReportMatchesQueryHandler CreateHandler()
    {
        return new GetPetReportMatchesQueryHandler(
            _repositoryMock.Object,
            _matchReadServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    private static PetReport CreatePetReport(Guid userId)
    {
        return PetReport.Create(
            userId: userId,
            reportType: ReportType.Lost,
            title: "Kayıp tekir kedi",
            description: "Gri ve beyaz tekir kedi kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Male,
            petName: "Atlas",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.2165,
                longitude: 28.9849));
    }
}