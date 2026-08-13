using FluentValidation.TestHelper;
using RescueLink.Application.Features.PetReports.Create;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests.Features.PetReports.Create;

public class CreatePetReportCommandValidatorTests
{
    private readonly CreatePetReportCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotContainErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldContainError_WhenTitleIsEmpty(
        string title)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Title = title
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Title);
    }

    [Fact]
    public void Validate_ShouldContainError_WhenTitleExceedsMaximumLength()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Title = new string('a', 151)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Title);
    }

    [Fact]
    public void Validate_ShouldContainError_WhenEventDateIsInFuture()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            EventDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.EventDate);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Validate_ShouldContainError_WhenLatitudeIsInvalid(
        double latitude)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Latitude = latitude
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Latitude);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Validate_ShouldContainError_WhenLongitudeIsInvalid(
        double longitude)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Longitude = longitude
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Longitude);
    }

    [Fact]
    public void Validate_ShouldContainError_WhenColorsAreSame()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PrimaryColor = AnimalColor.Gray,
            SecondaryColor = AnimalColor.Gray
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.SecondaryColor);
    }

    [Fact]
    public void Validate_ShouldContainError_WhenReportTypeIsInvalid()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            ReportType = (ReportType)99
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.ReportType);
    }

    private static CreatePetReportCommand CreateValidCommand()
    {
        return new CreatePetReportCommand(
            ReportType: ReportType.Lost,
            Title: "Kayıp kedi",
            Description: "Bursa Nilüfer bölgesinde kayboldu.",
            Species: AnimalSpecies.Cat,
            Gender: AnimalGender.Female,
            PetName: "Luna",
            Breed: "Tekir",
            PrimaryColor: AnimalColor.Gray,
            SecondaryColor: AnimalColor.White,
            EventDate: DateTimeOffset.UtcNow.AddHours(-1),
            Latitude: 40.195,
            Longitude: 29.060);
    }
}