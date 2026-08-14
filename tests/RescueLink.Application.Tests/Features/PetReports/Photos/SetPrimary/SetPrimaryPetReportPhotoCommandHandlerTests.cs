using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports.Photos.SetPrimary;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests
    .Features.PetReports.Photos.SetPrimary;

public sealed class SetPrimaryPetReportPhotoCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldChangePrimaryPhoto_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var report = CreateReportWithTwoPhotos(userId);
        var secondPhoto = report.Photos.Single(
            photo => photo.DisplayOrder == 1);

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

        var command = new SetPrimaryPetReportPhotoCommand(
            report.Id,
            secondPhoto.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        report.Photos.Single(photo => photo.Id == secondPhoto.Id)
            .IsPrimary.Should().BeTrue();

        report.Photos.Single(photo => photo.Id != secondPhoto.Id)
            .IsPrimary.Should().BeFalse();

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

        var command = new SetPrimaryPetReportPhotoCommand(
            Guid.NewGuid(),
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
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReportBelongsToAnotherUser()
    {
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var report = CreateReportWithTwoPhotos(ownerUserId);
        var photoId = report.Photos.First().Id;

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(currentUserId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        var command = new SetPrimaryPetReportPhotoCommand(
            report.Id,
            photoId);

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
    public async Task Handle_ShouldFail_WhenPhotoDoesNotBelongToReport()
    {
        var userId = Guid.NewGuid();
        var report = CreateReportWithTwoPhotos(userId);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = CreateHandler();

        var command = new SetPrimaryPetReportPhotoCommand(
            report.Id,
            Guid.NewGuid());

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private SetPrimaryPetReportPhotoCommandHandler CreateHandler()
    {
        return new SetPrimaryPetReportPhotoCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    private static PetReport CreateReportWithTwoPhotos(Guid userId)
    {
        var report = PetReport.Create(
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

        report.AddPhoto(
            "uploads/pet-reports/first.webp");

        report.AddPhoto(
            "uploads/pet-reports/second.webp");

        return report;
    }
}