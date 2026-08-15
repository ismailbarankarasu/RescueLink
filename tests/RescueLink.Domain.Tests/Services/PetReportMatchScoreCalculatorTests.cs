using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Services;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Domain.Tests.Services;

public sealed class PetReportMatchScoreCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturn100_WhenReportsFullyMatch()
    {
        var lostReport = CreateReport(
            ReportType.Lost,
            AnimalSpecies.Cat,
            AnimalGender.Male,
            "Tekir",
            AnimalColor.Gray,
            AnimalColor.White);

        var foundReport = CreateReport(
            ReportType.Found,
            AnimalSpecies.Cat,
            AnimalGender.Male,
            "tekir",
            AnimalColor.Gray,
            AnimalColor.White);

        var score = PetReportMatchScoreCalculator.Calculate(
            lostReport,
            foundReport,
            distanceMeters: 500);

        score.Should().Be(100);
    }

    [Fact]
    public void Calculate_ShouldReturn90_WhenColorsOverlap()
    {
        var lostReport = CreateReport(
            ReportType.Lost,
            AnimalSpecies.Cat,
            AnimalGender.Male,
            "Tekir",
            AnimalColor.Gray,
            AnimalColor.White);

        var foundReport = CreateReport(
            ReportType.Found,
            AnimalSpecies.Cat,
            AnimalGender.Male,
            "Tekir",
            AnimalColor.White,
            AnimalColor.Gray);

        var score = PetReportMatchScoreCalculator.Calculate(
            lostReport,
            foundReport,
            distanceMeters: 500);

        score.Should().Be(90);
    }

    [Fact]
    public void Calculate_ShouldReturnZero_WhenReportTypesAreSame()
    {
        var firstReport = CreateReport(
            reportType: ReportType.Lost);

        var secondReport = CreateReport(
            reportType: ReportType.Lost);

        var score = PetReportMatchScoreCalculator.Calculate(
            firstReport,
            secondReport,
            distanceMeters: 500);

        score.Should().Be(0);
    }

    [Fact]
    public void Calculate_ShouldReturnZero_WhenSpeciesAreDifferent()
    {
        var lostReport = CreateReport(
            reportType: ReportType.Lost,
            species: AnimalSpecies.Cat);

        var foundReport = CreateReport(
            reportType: ReportType.Found,
            species: AnimalSpecies.Dog);

        var score = PetReportMatchScoreCalculator.Calculate(
            lostReport,
            foundReport,
            distanceMeters: 500);

        score.Should().Be(0);
    }

    [Fact]
    public void Calculate_ShouldReturnZero_WhenDistanceIsTooFar()
    {
        var lostReport = CreateReport(
            reportType: ReportType.Lost);

        var foundReport = CreateReport(
            reportType: ReportType.Found);

        var score = PetReportMatchScoreCalculator.Calculate(
            lostReport,
            foundReport,
            distanceMeters: 10_001);

        score.Should().Be(0);
    }

    [Fact]
    public void Calculate_ShouldReturnZero_WhenReportIsNotActive()
    {
        var lostReport = CreateReport(
            reportType: ReportType.Lost);

        var foundReport = CreateReport(
            reportType: ReportType.Found);

        lostReport.Resolve();

        var score = PetReportMatchScoreCalculator.Calculate(
            lostReport,
            foundReport,
            distanceMeters: 500);

        score.Should().Be(0);
    }

    [Theory]
    [InlineData(500, 70)]
    [InlineData(2000, 65)]
    [InlineData(4000, 60)]
    [InlineData(8000, 55)]
    public void Calculate_ShouldApplyDistanceScore(
        double distanceMeters,
        int expectedScore)
    {
        var lostReport = CreateReport(
            reportType: ReportType.Lost,
            gender: AnimalGender.Unknown,
            breed: null,
            primaryColor: AnimalColor.Gray,
            secondaryColor: null);

        var foundReport = CreateReport(
            reportType: ReportType.Found,
            gender: AnimalGender.Unknown,
            breed: null,
            primaryColor: AnimalColor.Gray,
            secondaryColor: null);

        var score = PetReportMatchScoreCalculator.Calculate(
            lostReport,
            foundReport,
            distanceMeters);

        score.Should().Be(expectedScore);
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenDistanceIsNegative()
    {
        var lostReport = CreateReport(
            reportType: ReportType.Lost);

        var foundReport = CreateReport(
            reportType: ReportType.Found);

        var action = () =>
            PetReportMatchScoreCalculator.Calculate(
                lostReport,
                foundReport,
                distanceMeters: -1);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("distanceMeters");
    }

    private static PetReport CreateReport(
        ReportType reportType,
        AnimalSpecies species = AnimalSpecies.Cat,
        AnimalGender gender = AnimalGender.Male,
        string? breed = "Tekir",
        AnimalColor primaryColor = AnimalColor.Gray,
        AnimalColor? secondaryColor = AnimalColor.White)
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: reportType,
            title: "Hayvan ilanı",
            description: "Test açıklaması",
            species: species,
            gender: gender,
            petName: null,
            breed: breed,
            primaryColor: primaryColor,
            secondaryColor: secondaryColor,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(40.195, 29.060));
    }
}