using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports.Resolve;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features.PetReports.Resolve;

public sealed class ResolvePetReportCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldResolveReport_WhenRequestIsValid()
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
        var command = new ResolvePetReportCommand(report.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Resolved);

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

        var result = await handler.Handle(
            new ResolvePetReportCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _repositoryMock.Verify(
            x => x.GetByIdAsync(
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
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ResolvePetReportCommand(report.Id),
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

        report.Resolve();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new ResolvePetReportCommand(report.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Resolved);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ResolvePetReportCommandHandler CreateHandler()
    {
        return new ResolvePetReportCommandHandler(
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
            location: GeoLocation.Create(40.195, 29.060));
    }
}