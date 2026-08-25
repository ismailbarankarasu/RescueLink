using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Domain.Tests.Entities;

public class PetReportTests
{
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        // Arrange
        var emptyUserId = Guid.Empty;

        // Act
        Action act = () => PetReport.Create(
            userId: emptyUserId,
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("userId");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenTitleIsEmpty()
    {
        // Arrange
        var emptyTitle = "   ";

        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: emptyTitle,
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("title");
    }
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenDescriptionIsEmpty()
    {
        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "   ",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("description");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenEventDateIsInFuture()
    {
        // Arrange
        var futureEventDate = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: futureEventDate,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("eventDate");
    }

    [Fact]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenReportTypeIsInvalid()
    {
        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: (ReportType)99,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("reportType");
    }

    [Fact]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenSpeciesIsInvalid()
    {
        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp hayvan",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: (AnimalSpecies)99,
            gender: AnimalGender.Unknown,
            petName: null,
            breed: null,
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("species");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenColorsAreSame()
    {
        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Found,
            title: "Bulunan kedi",
            description: "Bursa Nilüfer bölgesinde bulundu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Unknown,
            petName: null,
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.Gray,
            eventDate: DateTimeOffset.UtcNow,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("secondaryColor");
    }

    [Fact]
    public void Create_ShouldCreateActivePetReport_WhenInputIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventDate = DateTimeOffset.UtcNow.AddHours(-2);

        // Act
        var report = PetReport.Create(
            userId: userId,
            reportType: ReportType.Lost,
            title: "  Kayıp kedi  ",
            description: "  Bursa Nilüfer bölgesinde kayboldu.  ",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "  Luna  ",
            breed: "  Tekir  ",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: eventDate,
            location: GeoLocation.Create(40.195, 29.060));

        // Assert
        report.Id.Should().NotBeEmpty();
        report.UserId.Should().Be(userId);
        report.ReportType.Should().Be(ReportType.Lost);
        report.Status.Should().Be(ReportStatus.Active);

        report.Title.Should().Be("Kayıp kedi");
        report.Description.Should().Be(
            "Bursa Nilüfer bölgesinde kayboldu.");

        report.Species.Should().Be(AnimalSpecies.Cat);
        report.Gender.Should().Be(AnimalGender.Female);
        report.PetName.Should().Be("Luna");
        report.Breed.Should().Be("Tekir");

        report.PrimaryColor.Should().Be(AnimalColor.Gray);
        report.SecondaryColor.Should().Be(AnimalColor.White);
        report.EventDate.Should().Be(eventDate);
    }

    [Fact]
    public void Resolve_ShouldSetStatusToResolved_WhenReportIsActive()
    {
        // Arrange
        var report = CreateValidReport();
        var beforeResolve = DateTimeOffset.UtcNow;

        // Act
        report.Resolve();

        var afterResolve = DateTimeOffset.UtcNow;

        // Assert
        report.Status.Should().Be(ReportStatus.Resolved);

        report.UpdatedAt.Should()
            .NotBeNull()
            .And.BeOnOrAfter(beforeResolve)
            .And.BeOnOrBefore(afterResolve);
    }

    [Fact]
    public void Resolve_ShouldThrowInvalidOperationException_WhenReportIsAlreadyResolved()
    {
        // Arrange
        var report = CreateValidReport();
        report.Resolve();

        // Act
        Action act = report.Resolve;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Only active reports can be resolved.");
    }

    private static PetReport CreateValidReport()
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(40.195, 29.060));
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_WhenReportIsActive()
    {
        // Arrange
        var report = CreateValidReport();
        var beforeCancel = DateTimeOffset.UtcNow;

        // Act
        report.Cancel();

        var afterCancel = DateTimeOffset.UtcNow;

        // Assert
        report.Status.Should().Be(ReportStatus.Cancelled);

        report.UpdatedAt.Should()
            .NotBeNull()
            .And.BeOnOrAfter(beforeCancel)
            .And.BeOnOrBefore(afterCancel);
    }

    [Fact]
    public void Cancel_ShouldThrowInvalidOperationException_WhenReportIsResolved()
    {
        // Arrange
        var report = CreateValidReport();
        report.Resolve();

        // Act
        Action act = report.Cancel;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Only active reports can be cancelled.");
    }

    [Fact]
    public void Resolve_ShouldThrowInvalidOperationException_WhenReportIsCancelled()
    {
        // Arrange
        var report = CreateValidReport();
        report.Cancel();

        // Act
        Action act = report.Resolve;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Only active reports can be resolved.");
    }

    [Fact]
    public void Create_ShouldAssignLocation_WhenLocationIsValid()
    {
        // Arrange
        var location = GeoLocation.Create(40.195, 29.060);

        // Act
        var report = PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: location);

        // Assert
        report.Location.Should().Be(location);
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenLocationIsNull()
    {
        // Act
        Action act = () => PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: null!);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("location");
    }

    [Fact]
    public void AddPhoto_ShouldAddFirstPhotoAsPrimary()
    {
        // Arrange
        var report = CreateValidReport();

        // Act
        report.AddPhoto("pet-reports/report-1/photo-1.webp");

        // Assert
        report.Photos.Should().ContainSingle();

        var photo = report.Photos.Single();

        photo.PetReportId.Should().Be(report.Id);
        photo.StorageKey.Should()
            .Be("pet-reports/report-1/photo-1.webp");
        photo.IsPrimary.Should().BeTrue();
        photo.DisplayOrder.Should().Be(0);
        report.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddPhoto_ShouldAddSecondPhotoAsNonPrimary()
    {
        // Arrange
        var report = CreateValidReport();

        // Act
        report.AddPhoto("photo-1.webp");
        report.AddPhoto("photo-2.webp");

        // Assert
        report.Photos.Should().HaveCount(2);

        var secondPhoto = report.Photos.Single(
            photo => photo.DisplayOrder == 1);

        secondPhoto.IsPrimary.Should().BeFalse();
        secondPhoto.StorageKey.Should().Be("photo-2.webp");
    }

    [Fact]
    public void AddPhoto_ShouldThrowInvalidOperationException_WhenStorageKeyAlreadyExists()
    {
        // Arrange
        var report = CreateValidReport();
        report.AddPhoto("pet-reports/photo-1.webp");

        // Act
        Action act = () =>
            report.AddPhoto("PET-REPORTS/PHOTO-1.WEBP");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "The same photo cannot be added more than once.");

        report.Photos.Should().ContainSingle();
    }

    [Fact]
    public void AddPhoto_ShouldThrowInvalidOperationException_WhenMaximumPhotoCountIsExceeded()
    {
        // Arrange
        var report = CreateValidReport();

        for (var index = 1; index <= 5; index++)
        {
            report.AddPhoto($"photo-{index}.webp");
        }

        // Act
        Action act = () => report.AddPhoto("photo-6.webp");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "A report can contain at most 5 photos.");

        report.Photos.Should().HaveCount(5);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddPhoto_ShouldThrowArgumentException_WhenStorageKeyIsEmpty(
    string storageKey)
    {
        // Arrange
        var report = CreateValidReport();

        // Act
        Action act = () => report.AddPhoto(storageKey);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("storageKey");

        report.Photos.Should().BeEmpty();
    }

    [Fact]
    public void SetPrimaryPhoto_ShouldChangePrimaryPhoto_WhenPhotoBelongsToReport()
    {
        // Arrange
        var report = CreateValidReport();
        report.AddPhoto("photo-1.webp");
        report.AddPhoto("photo-2.webp");

        var secondPhotoId = report.Photos
            .Single(photo => photo.DisplayOrder == 1)
            .Id;

        // Act
        report.SetPrimaryPhoto(secondPhotoId);

        // Assert
        report.Photos.Single(photo => photo.Id == secondPhotoId)
            .IsPrimary.Should().BeTrue();

        report.Photos.Single(photo => photo.DisplayOrder == 0)
            .IsPrimary.Should().BeFalse();

        report.Photos.Count(photo => photo.IsPrimary)
            .Should().Be(1);

        report.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetPrimaryPhoto_ShouldThrowInvalidOperationException_WhenPhotoDoesNotBelongToReport()
    {
        // Arrange
        var report = CreateValidReport();
        report.AddPhoto("photo-1.webp");

        // Act
        Action act = () =>
            report.SetPrimaryPhoto(Guid.NewGuid());

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Photo does not belong to this report.");

        report.Photos.Single().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void RemovePhoto_ShouldRemovePhotoAndReorderRemainingPhotos()
    {
        // Arrange
        var report = CreateValidReport();
        report.AddPhoto("photo-1.webp");
        report.AddPhoto("photo-2.webp");
        report.AddPhoto("photo-3.webp");

        var secondPhotoId = report.Photos
            .Single(photo => photo.DisplayOrder == 1)
            .Id;

        // Act
        report.RemovePhoto(secondPhotoId);

        // Assert
        report.Photos.Should().HaveCount(2);

        report.Photos
            .OrderBy(photo => photo.DisplayOrder)
            .Select(photo => photo.StorageKey)
            .Should()
            .Equal("photo-1.webp", "photo-3.webp");

        report.Photos
            .OrderBy(photo => photo.DisplayOrder)
            .Select(photo => photo.DisplayOrder)
            .Should()
            .Equal(0, 1);
    }

    [Fact]
    public void RemovePhoto_ShouldAssignNewPrimary_WhenPrimaryPhotoIsRemoved()
    {
        // Arrange
        var report = CreateValidReport();
        report.AddPhoto("photo-1.webp");
        report.AddPhoto("photo-2.webp");

        var primaryPhotoId = report.Photos
            .Single(photo => photo.IsPrimary)
            .Id;

        // Act
        report.RemovePhoto(primaryPhotoId);

        // Assert
        report.Photos.Should().ContainSingle();
        report.Photos.Single().IsPrimary.Should().BeTrue();
        report.Photos.Single().DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void RemovePhoto_ShouldThrowInvalidOperationException_WhenPhotoDoesNotBelongToReport()
    {
        // Arrange
        var report = CreateValidReport();
        report.AddPhoto("photo-1.webp");

        // Act
        Action act = () =>
            report.RemovePhoto(Guid.NewGuid());

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Photo does not belong to this report.");

        report.Photos.Should().ContainSingle();
    }

    [Fact]
    public void Create_ShouldRaisePetReportCreatedDomainEvent()
    {
        var report = PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Gri renkli kedi kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(40.195, 29.060));

        var domainEvent = report.DomainEvents
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<PetReportCreatedDomainEvent>()
            .Subject;

        domainEvent.PetReportId.Should().Be(report.Id);
    }

    [Fact]
    public void Archive_ShouldArchiveReport()
    {
        // Arrange
        var report = CreateReport();

        var beforeArchive =
            DateTimeOffset.UtcNow;

        // Act
        report.Archive();

        // Assert
        report.IsArchived.Should().BeTrue();

        report.ArchivedAt.Should()
            .NotBeNull();

        report.ArchivedAt.Should()
            .BeOnOrAfter(beforeArchive);

        report.UpdatedAt.Should()
            .NotBeNull();

        report.UpdatedAt.Should()
            .Be(report.ArchivedAt);
    }

    [Fact]
    public void Archive_ShouldBeIdempotent_WhenAlreadyArchived()
    {
        // Arrange
        var report = CreateReport();

        report.Archive();

        var firstArchivedAt =
            report.ArchivedAt;

        var firstUpdatedAt =
            report.UpdatedAt;

        // Act
        report.Archive();

        // Assert
        report.IsArchived.Should().BeTrue();

        report.ArchivedAt.Should()
            .Be(firstArchivedAt);

        report.UpdatedAt.Should()
            .Be(firstUpdatedAt);
    }
    private static PetReport CreateReport()
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
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

    [Fact]
    public void ArchivedReport_ShouldRejectAllModifications()
    {
        // Arrange
        var report = CreateReport();

        report.AddPhoto(
            "uploads/pet-reports/photo.webp");

        var photoId =
            report.Photos.Single().Id;

        report.Archive();

        var newLocation =
            GeoLocation.Create(
                latitude: 40.200,
                longitude: 29.100);

        Action[] actions =
        [
            () => report.UpdateDetails(
            title: "Güncellenmiş başlık",
            description: "Güncellenmiş açıklama",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate:
                DateTimeOffset.UtcNow.AddHours(-2),
            location: newLocation),

        () => report.Resolve(),

        () => report.Cancel(),

        () => report.AddPhoto(
            "uploads/pet-reports/second.webp"),

        () => report.SetPrimaryPhoto(photoId),

        () => report.RemovePhoto(photoId)
        ];

        // Act & Assert
        foreach (var action in actions)
        {
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "Archived pet reports cannot be modified.");
        }
    }

    [Fact]
    public void CanAddPhoto_ShouldBeFalse_WhenReportIsArchived()
    {
        // Arrange
        var report = CreateReport();

        report.Archive();

        // Act
        var canAddPhoto =
            report.CanAddPhoto;

        // Assert
        canAddPhoto.Should().BeFalse();
    }
}