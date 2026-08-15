using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports.Cancel;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features.PetReports.Cancel;

public sealed class CancelPetReportCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldCancelReport_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var report = CreatePetReport(userId);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new CancelPetReportCommand(report.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Cancelled);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
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

        var command = new CancelPetReportCommand(
            Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
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
            .Setup(x => x.GetByIdAsync(
                reportId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetReport?)null);

        var handler = CreateHandler();
        var command = new CancelPetReportCommand(reportId);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
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
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();
        var command = new CancelPetReportCommand(report.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Active);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReportIsNotActive()
    {
        var userId = Guid.NewGuid();
        var report = CreatePetReport(userId);

        report.Cancel();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();
        var command = new CancelPetReportCommand(report.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Cancelled);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CancelPetReportCommandHandler CreateHandler()
    {
        return new CancelPetReportCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    private static PetReport CreatePetReport(Guid userId)
    {
        return PetReport.Create(
            userId: userId,
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Gri renkli kedi kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: null,
            eventDate: DateTimeOffset.UtcNow.AddDays(-1),
            location: GeoLocation.Create(
                latitude: 40.195,
                longitude: 29.060));
    }
}