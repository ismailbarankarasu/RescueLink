using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports.Matching;
using RescueLink.Application.Features.PetReports.Matching.Recalculate;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;
namespace RescueLink.Application.Tests.Features.PetReports.Matching;

public sealed class RecalculatePetReportMatchesCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _reportRepositoryMock =
        new();

    private readonly Mock<IPetReportMatchRepository> _matchRepositoryMock =
        new();

    private readonly Mock<IPetReportMatchCandidateReadService>
        _candidateReadServiceMock = new();

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    public RecalculatePetReportMatchesCommandHandlerTests()
    {
        _unitOfWorkMock
            .Setup(unitOfWork =>
                unitOfWork.ExecuteInTransactionAsync(
                    It.IsAny<
                        Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
            .Returns(
                (
                    Func<CancellationToken, Task> operation,
                    CancellationToken cancellationToken
                ) => operation(cancellationToken));

        _unitOfWorkMock
            .Setup(unitOfWork =>
                unitOfWork.AcquireTransactionLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Theory]
    [InlineData(ReportType.Lost)]
    [InlineData(ReportType.Found)]
    public async Task Handle_ShouldCreateMatch_WhenCandidateIsSuitable(ReportType sourceReportType)
    {
        var candidateReportType =
            sourceReportType == ReportType.Lost
                ? ReportType.Found
                : ReportType.Lost;

        var sourceReport = CreateReport(
            sourceReportType,
            Guid.NewGuid());

        var candidateReport = CreateReport(
            candidateReportType,
            Guid.NewGuid());

        const double distanceMeters = 500;

        _reportRepositoryMock
            .Setup(x => x.GetByIdReadOnlyAsync(
                sourceReport.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceReport);

        _candidateReadServiceMock
            .Setup(x => x.GetCandidatesAsync(
                sourceReport.Id,
                sourceReport.UserId,
                candidateReportType,
                sourceReport.Species,
                sourceReport.Location.Latitude,
                sourceReport.Location.Longitude,
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PetReportMatchCandidate(
                    candidateReport.Id,
                    distanceMeters)
            ]);

        _reportRepositoryMock
            .Setup(x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidateReport]);

        _matchRepositoryMock
            .Setup(x => x.GetExistingCounterpartIdsAsync(
                sourceReport.Id,
                sourceReport.ReportType,
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid>());

        IReadOnlyCollection<PetReportMatch>? capturedMatches = null;

        _matchRepositoryMock
            .Setup(x => x.AddRangeAsync(
                It.IsAny<IReadOnlyCollection<PetReportMatch>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                IReadOnlyCollection<PetReportMatch>,
                CancellationToken>(
                (matches, _) => capturedMatches = matches)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        var command = new RecalculatePetReportMatchesCommand(sourceReport.Id);
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        Assert.NotNull(capturedMatches);
        Assert.Single(capturedMatches);

        var match = capturedMatches.Single();

        var expectedLostReportId =
            sourceReportType == ReportType.Lost
                ? sourceReport.Id
                : candidateReport.Id;

        var expectedFoundReportId =
            sourceReportType == ReportType.Found
                ? sourceReport.Id
                : candidateReport.Id;

        Assert.Equal(
            expectedLostReportId,
            match.LostReportId);

        Assert.Equal(
            expectedFoundReportId,
            match.FoundReportId);

        Assert.Equal(100, match.Score);
        Assert.Equal(distanceMeters, match.DistanceMeters);
        Assert.Equal(MatchStatus.Suggested, match.Status);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.ExecuteInTransactionAsync(
                    It.IsAny<
                        Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldStop_WhenNoCandidateExists()
    {
        var sourceReport = CreateReport(
            ReportType.Lost,
            Guid.NewGuid());

        _reportRepositoryMock
            .Setup(x => x.GetByIdReadOnlyAsync(
                sourceReport.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceReport);

        _candidateReadServiceMock
            .Setup(x => x.GetCandidatesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ReportType>(),
                It.IsAny<AnimalSpecies>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = CreateHandler();

        var command = new RecalculatePetReportMatchesCommand(sourceReport.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _matchRepositoryMock.Verify(
            x => x.AddRangeAsync(
                It.IsAny<IReadOnlyCollection<PetReportMatch>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.ExecuteInTransactionAsync(
                    It.IsAny<
                        Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateDuplicateMatch()
    {
        var sourceReport = CreateReport(
            ReportType.Lost,
            Guid.NewGuid());

        var candidateReport = CreateReport(
            ReportType.Found,
            Guid.NewGuid());

        _reportRepositoryMock
            .Setup(x => x.GetByIdReadOnlyAsync(
                sourceReport.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceReport);

        _candidateReadServiceMock
            .Setup(x => x.GetCandidatesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ReportType>(),
                It.IsAny<AnimalSpecies>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PetReportMatchCandidate(
                    candidateReport.Id,
                    500)
            ]);

        _reportRepositoryMock
            .Setup(x => x.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidateReport]);

        _matchRepositoryMock
            .Setup(x => x.GetExistingCounterpartIdsAsync(
                sourceReport.Id,
                sourceReport.ReportType,
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new HashSet<Guid>
                {
                    candidateReport.Id
                });

        var handler = CreateHandler();

        var command = new RecalculatePetReportMatchesCommand(sourceReport.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _matchRepositoryMock.Verify(
            x => x.AddRangeAsync(
                It.IsAny<IReadOnlyCollection<PetReportMatch>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.ExecuteInTransactionAsync(
                    It.IsAny<
                        Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private RecalculatePetReportMatchesCommandHandler CreateHandler()
    {
        return new RecalculatePetReportMatchesCommandHandler(
            _reportRepositoryMock.Object,
            _matchRepositoryMock.Object,
            _candidateReadServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    private static PetReport CreateReport(
        ReportType reportType,
        Guid userId)
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