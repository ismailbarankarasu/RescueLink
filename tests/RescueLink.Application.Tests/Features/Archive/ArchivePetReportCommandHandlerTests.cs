using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.Archive;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests
    .Features.PetReports.Archive;

public sealed class ArchivePetReportCommandHandlerTests
{
    private readonly Mock<IPetReportRepository>
        _petReportRepositoryMock = new();

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldArchiveReport_WhenUserOwnsReport()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var report = CreateReport(userId);

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _petReportRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingArchivedAsync(
                    report.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ArchivePetReportCommand(report.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        report.IsArchived.Should().BeTrue();
        report.ArchivedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
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
            new ArchivePetReportCommand(
                Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PetReportErrors.Unauthenticated);

        _petReportRepositoryMock.Verify(
            repository =>
                repository.GetByIdIncludingArchivedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReportDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _petReportRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingArchivedAsync(
                    reportId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetReport?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ArchivePetReportCommand(reportId),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            PetReportErrors.NotFound(reportId).Code);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotOwnReport()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var report = CreateReport(ownerId);

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(currentUserId);

        _petReportRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingArchivedAsync(
                    report.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new ArchivePetReportCommand(report.Id),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            PetReportErrors.Forbidden);

        report.IsArchived.Should().BeFalse();

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ArchivePetReportCommandHandler CreateHandler()
    {
        return new ArchivePetReportCommandHandler(
            _petReportRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    private static PetReport CreateReport(
        Guid userId)
    {
        return PetReport.Create(
            userId: userId,
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description:
                "Gri renkli kedimiz kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate:
                DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.195,
                longitude: 29.060));
    }
}