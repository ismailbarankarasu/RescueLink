using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

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
            eventDate: DateTimeOffset.UtcNow);

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
            eventDate: DateTimeOffset.UtcNow);

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
            eventDate: DateTimeOffset.UtcNow);

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
            eventDate: futureEventDate);

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
            eventDate: DateTimeOffset.UtcNow);

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
            eventDate: DateTimeOffset.UtcNow);

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
            eventDate: DateTimeOffset.UtcNow);

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
            eventDate: eventDate);

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
}