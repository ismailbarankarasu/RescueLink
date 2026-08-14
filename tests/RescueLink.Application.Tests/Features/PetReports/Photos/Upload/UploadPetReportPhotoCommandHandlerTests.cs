using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Abstractions.Storage;
using RescueLink.Application.Features.PetReports.Photos.Upload;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features.PetReports.Photos.Upload;

public sealed class UploadPetReportPhotoCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();

    private UploadPetReportPhotoCommandHandler CreateHandler()
    {
        return new UploadPetReportPhotoCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddPhoto_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var report = CreatePetReport(userId);
        const string storageKey =
            "uploads/pet-reports/photo.jpg";

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        _fileStorageServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<FileUpload>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageKey);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateCommand(report.Id, content);
        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        report.Photos.Should().ContainSingle();

        var photo = report.Photos.Single();

        result.Value.Should().Be(photo.Id);
        photo.StorageKey.Should().Be(storageKey);
        photo.IsPrimary.Should().BeTrue();
        photo.DisplayOrder.Should().Be(0);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateCommand(
            Guid.NewGuid(),
            content);

        var handler = CreateHandler();

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
            x => x.UploadAsync(
                It.IsAny<FileUpload>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReportBelongsToAnotherUser()
    {
        var currentUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var report = CreatePetReport(ownerUserId);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(currentUserId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateCommand(report.Id, content);
        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _fileStorageServiceMock.Verify(
            x => x.UploadAsync(
                It.IsAny<FileUpload>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPhotoLimitIsReached()
    {
        var userId = Guid.NewGuid();
        var report = CreatePetReport(userId);

        for (var index = 0;
             index < PetReport.MaximumPhotoCount;
             index++)
        {
            report.AddPhoto(
                $"uploads/pet-reports/photo-{index}.jpg");
        }

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateCommand(report.Id, content);
        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _fileStorageServiceMock.Verify(
            x => x.UploadAsync(
                It.IsAny<FileUpload>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteUploadedFile_WhenSavingFails()
    {
        var userId = Guid.NewGuid();
        var report = CreatePetReport(userId);
        const string storageKey =
            "uploads/pet-reports/photo.jpg";

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                report.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        _fileStorageServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<FileUpload>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageKey);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Database save failed."));

        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateCommand(report.Id, content);
        var handler = CreateHandler();

        var action = async () => await handler.Handle(
            command,
            CancellationToken.None);

        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        _fileStorageServiceMock.Verify(
            x => x.DeleteAsync(
                storageKey,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static UploadPetReportPhotoCommand CreateCommand(
        Guid reportId,
        Stream content)
    {
        return new UploadPetReportPhotoCommand(
            PetReportId: reportId,
            Content: content,
            FileName: "pet-photo.jpg",
            ContentType: "image/jpeg",
            Length: content.Length);
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