using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReportMatches;
using RescueLink.Application.Features.PetReportMatches.GetContact;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features
    .PetReportMatches.GetContact;

public sealed class GetMatchContactQueryHandlerTests
{
    private readonly Mock<IPetReportMatchRepository>
        _matchRepositoryMock = new();

    private readonly Mock<IPetReportRepository>
        _reportRepositoryMock = new();

    private readonly Mock<IIdentityService>
        _identityServiceMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnFoundOwnerContact_WhenLostOwnerRequests()
    {
        // Arrange
        var lostOwnerId = Guid.NewGuid();
        var foundOwnerId = Guid.NewGuid();

        var lostReport = CreateReport(
            lostOwnerId,
            ReportType.Lost);

        var foundReport = CreateReport(
            foundOwnerId,
            ReportType.Found);

        var match = CreateConfirmedMatch(
            lostReport,
            foundReport);

        var contact = new UserContactInfo(
            foundOwnerId,
            "Ayşe",
            "Yılmaz",
            "ayse@example.com");

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(lostOwnerId);

        SetupMatchAndReports(
            match,
            lostReport,
            foundReport);

        _identityServiceMock
            .Setup(service => service.GetUserContactAsync(
                foundOwnerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMatchContactQuery(match.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.UserId.Should().Be(foundOwnerId);
        result.Value.FirstName.Should().Be("Ayşe");
        result.Value.LastName.Should().Be("Yılmaz");
        result.Value.Email.Should().Be("ayse@example.com");
    }

    [Fact]
    public async Task Handle_ShouldReturnLostOwnerContact_WhenFoundOwnerRequests()
    {
        // Arrange
        var lostOwnerId = Guid.NewGuid();
        var foundOwnerId = Guid.NewGuid();

        var lostReport = CreateReport(
            lostOwnerId,
            ReportType.Lost);

        var foundReport = CreateReport(
            foundOwnerId,
            ReportType.Found);

        var match = CreateConfirmedMatch(
            lostReport,
            foundReport);

        var contact = new UserContactInfo(
            lostOwnerId,
            "Mehmet",
            "Demir",
            "mehmet@example.com");

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(foundOwnerId);

        SetupMatchAndReports(
            match,
            lostReport,
            foundReport);

        _identityServiceMock
            .Setup(service => service.GetUserContactAsync(
                lostOwnerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMatchContactQuery(match.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.UserId.Should().Be(lostOwnerId);
        result.Value.Email.Should().Be("mehmet@example.com");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMatchContactQuery(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should()
            .Be("Authentication.Unauthenticated");

        _matchRepositoryMock.Verify(
            repository => repository.GetByIdReadOnlyAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMatchDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(userId);

        _matchRepositoryMock
            .Setup(repository => repository.GetByIdReadOnlyAsync(
                matchId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetReportMatch?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMatchContactQuery(matchId),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PetReportMatch.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotOwnEitherReport()
    {
        // Arrange
        var lostReport = CreateReport(
            Guid.NewGuid(),
            ReportType.Lost);

        var foundReport = CreateReport(
            Guid.NewGuid(),
            ReportType.Found);

        var match = CreateConfirmedMatch(
            lostReport,
            foundReport);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(Guid.NewGuid());

        SetupMatchAndReports(
            match,
            lostReport,
            foundReport);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMatchContactQuery(match.Id),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PetReportMatch.Forbidden");

        _identityServiceMock.Verify(
            service => service.GetUserContactAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMatchIsNotConfirmed()
    {
        // Arrange
        var lostOwnerId = Guid.NewGuid();

        var lostReport = CreateReport(
            lostOwnerId,
            ReportType.Lost);

        var foundReport = CreateReport(
            Guid.NewGuid(),
            ReportType.Found);

        var match = PetReportMatch.Create(
            lostReport.Id,
            foundReport.Id,
            score: 90,
            distanceMeters: 100);

        match.ClearDomainEvents();

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(lostOwnerId);

        SetupMatchAndReports(
            match,
            lostReport,
            foundReport);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMatchContactQuery(match.Id),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should()
            .Be("PetReportMatch.ContactNotAvailable");

        _identityServiceMock.Verify(
            service => service.GetUserContactAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetMatchContactQueryHandler CreateHandler()
    {
        return new GetMatchContactQueryHandler(
            _matchRepositoryMock.Object,
            _reportRepositoryMock.Object,
            _identityServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    private void SetupMatchAndReports(
        PetReportMatch match,
        PetReport lostReport,
        PetReport foundReport)
    {
        _matchRepositoryMock
            .Setup(repository => repository.GetByIdReadOnlyAsync(
                match.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        _reportRepositoryMock
            .Setup(repository => repository.GetByIdsReadOnlyAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                lostReport,
                foundReport
            });
    }

    private static PetReportMatch CreateConfirmedMatch(
        PetReport lostReport,
        PetReport foundReport)
    {
        var match = PetReportMatch.Create(
            lostReport.Id,
            foundReport.Id,
            score: 100,
            distanceMeters: 50);

        match.ClearDomainEvents();

        match.Confirm(lostReport.Id);
        match.Confirm(foundReport.Id);

        match.ClearDomainEvents();

        return match;
    }

    private static PetReport CreateReport(
        Guid userId,
        ReportType reportType)
    {
        var report = PetReport.Create(
            userId: userId,
            reportType: reportType,
            title: "Test ilanı",
            description: "Test ilanı açıklaması",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Unknown,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddDays(-1),
            location: GeoLocation.Create(
                latitude: 40.195,
                longitude: 29.060));

        report.ClearDomainEvents();

        return report;
    }
}