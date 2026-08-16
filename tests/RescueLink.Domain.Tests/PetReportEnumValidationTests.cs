using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Domain.Tests.Entities;

public sealed class PetReportEnumValidationTests
{
    [Fact]
    public void Create_ShouldThrow_WhenGenderIsInvalid()
    {
        var action = () => CreateReport(
            gender: (AnimalGender)999,
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("gender");
    }

    [Fact]
    public void Create_ShouldThrow_WhenPrimaryColorIsInvalid()
    {
        var action = () => CreateReport(
            gender: AnimalGender.Male,
            primaryColor: (AnimalColor)999,
            secondaryColor: AnimalColor.White);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("primaryColor");
    }

    [Fact]
    public void Create_ShouldThrow_WhenSecondaryColorIsInvalid()
    {
        var action = () => CreateReport(
            gender: AnimalGender.Male,
            primaryColor: AnimalColor.Gray,
            secondaryColor: (AnimalColor)999);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("secondaryColor");
    }

    private static PetReport CreateReport(
        AnimalGender gender,
        AnimalColor primaryColor,
        AnimalColor? secondaryColor)
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp hayvan ilanı",
            description: "Hayvanımızı arıyoruz.",
            species: AnimalSpecies.Cat,
            gender: gender,
            petName: "Pamuk",
            breed: "Tekir",
            primaryColor: primaryColor,
            secondaryColor: secondaryColor,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.2165,
                longitude: 28.9849));
    }
}