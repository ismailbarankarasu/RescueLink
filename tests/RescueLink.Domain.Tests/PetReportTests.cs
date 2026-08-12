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
}