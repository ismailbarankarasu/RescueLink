using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Abstractions.Storage;
using RescueLink.Application.Features.PetReports.Photos.Delete;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests
    .Features.PetReports.Photos.Delete;

public sealed class DeletePetReportPhotoCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();

    [Fact]
    public async Task Handle_ShouldDeletePhoto_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var report = CreateReportWithTwoPhotos(userId);

        var primaryPhoto = report.Photos.Single(
            photo => photo.IsPrimary);

        var remainingPhoto = report.Photos.Single(
            photo => !photo.IsPrimary);

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

        var command = new DeletePetReportPhotoCommand(
            report.Id,
            primaryPhoto.Id);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        report.Photos.Should().ContainSingle();

        var currentPhoto = report.Photos.Single();

        currentPhoto.Id.Should().Be(remainingPhoto.Id);
        currentPhoto.IsPrimary.Should().BeTrue();
        currentPhoto.DisplayOrder.Should().Be(0);

        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(
                primaryPhoto.StorageKey,
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

        var command = new DeletePetReportPhotoCommand(
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

        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>(),
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

        var command = new DeletePetReportPhotoCommand(
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

        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotDeletePhysicalFile_WhenSavingFails()
    {
        var userId = Guid.NewGuid();
        var report = CreateReportWithTwoPhotos(userId);
        var photo = report.Photos.First();

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
            .ThrowsAsync(
                new InvalidOperationException(
                    "Database save failed."));

        var handler = CreateHandler();

        var command = new DeletePetReportPhotoCommand(
            report.Id,
            photo.Id);

        var action = async () => await handler.Handle(
            command,
            CancellationToken.None);

        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private DeletePetReportPhotoCommandHandler CreateHandler()
    {
        return new DeletePetReportPhotoCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object);
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