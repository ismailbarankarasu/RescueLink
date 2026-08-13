using FluentAssertions;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Domain.Tests.ValueObjects;

public class GeoLocationTests
{
    [Fact]
    public void Create_ShouldCreateLocation_WhenCoordinatesAreValid()
    {
        // Arrange
        const double latitude = 40.195;
        const double longitude = 29.060;

        // Act
        var location = GeoLocation.Create(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(90)]
    public void Create_ShouldAcceptLatitudeBoundaryValues(
        double latitude)
    {
        // Act
        var location = GeoLocation.Create(
            latitude,
            longitude: 29.060);

        // Assert
        location.Latitude.Should().Be(latitude);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_ShouldThrow_WhenLatitudeIsInvalid(
        double latitude)
    {
        // Act
        Action act = () => GeoLocation.Create(
            latitude,
            longitude: 29.060);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Create_ShouldThrow_WhenLongitudeIsInvalid(
        double longitude)
    {
        // Act
        Action act = () => GeoLocation.Create(
            latitude: 40.195,
            longitude);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Fact]
    public void EqualCoordinates_ShouldCreateEqualLocations()
    {
        // Arrange
        var firstLocation = GeoLocation.Create(40.195, 29.060);
        var secondLocation = GeoLocation.Create(40.195, 29.060);

        // Assert
        firstLocation.Should().Be(secondLocation);
    }
}