using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Domain.Tests.Entities;

public sealed class PetReportUpdateTests
{
    [Fact]
    public void UpdateDetails_ShouldUpdateReport_WhenReportIsActive()
    {
        var report = CreateActiveReport();

        var newEventDate =
            DateTimeOffset.UtcNow.AddHours(-2);

        var newLocation = GeoLocation.Create(
            latitude: 40.2200,
            longitude: 28.9800);

        report.UpdateDetails(
            title: "Güncellenmiş kayıp kedi ilanı",
            description: "Kedimiz son olarak parkta görüldü.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.White,
            secondaryColor: AnimalColor.Gray,
            eventDate: newEventDate,
            location: newLocation);

        report.Title.Should()
            .Be("Güncellenmiş kayıp kedi ilanı");

        report.Description.Should()
            .Be("Kedimiz son olarak parkta görüldü.");

        report.Species.Should().Be(AnimalSpecies.Cat);
        report.Gender.Should().Be(AnimalGender.Female);
        report.PetName.Should().Be("Luna");
        report.Breed.Should().Be("Tekir");
        report.PrimaryColor.Should().Be(AnimalColor.White);
        report.SecondaryColor.Should().Be(AnimalColor.Gray);
        report.EventDate.Should().Be(newEventDate);
        report.Location.Latitude.Should().Be(40.2200);
        report.Location.Longitude.Should().Be(28.9800);
        report.Status.Should().Be(ReportStatus.Active);
        report.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldNormalizeOptionalTexts()
    {
        var report = CreateActiveReport();

        report.UpdateDetails(
            title: "  Güncel ilan  ",
            description: "  Güncel açıklama  ",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Unknown,
            petName: "   ",
            breed: "  Tekir  ",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.2165,
                longitude: 28.9849));

        report.Title.Should().Be("Güncel ilan");
        report.Description.Should().Be("Güncel açıklama");
        report.PetName.Should().BeNull();
        report.Breed.Should().Be("Tekir");
    }

    [Theory]
    [InlineData(ReportStatus.Resolved)]
    [InlineData(ReportStatus.Cancelled)]
    public void UpdateDetails_ShouldThrow_WhenReportIsNotActive(
        ReportStatus status)
    {
        var report = CreateActiveReport();

        if (status == ReportStatus.Resolved)
        {
            report.Resolve();
        }
        else
        {
            report.Cancel();
        }

        var action = () => report.UpdateDetails(
            title: "Yeni başlık",
            description: "Yeni açıklama",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Male,
            petName: null,
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(
                latitude: 40.2165,
                longitude: 28.9849));

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Only active reports can be updated.");
    }

    private static PetReport CreateActiveReport()
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp tekir kedi",
            description: "Gri ve beyaz tekir kedimiz kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Male,
            petName: "Pamuk",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-3),
            location: GeoLocation.Create(
                latitude: 40.2165,
                longitude: 28.9849));
    }
}