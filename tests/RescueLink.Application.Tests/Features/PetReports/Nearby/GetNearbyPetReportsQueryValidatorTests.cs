using FluentAssertions;
using RescueLink.Application.Features.PetReports.Nearby;

namespace RescueLink.Application.Tests.Features.PetReports.Nearby;

public sealed class GetNearbyPetReportsQueryValidatorTests
{
    private readonly GetNearbyPetReportsQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenQueryIsValid()
    {
        var query = new GetNearbyPetReportsQuery(
            Latitude: 40.195,
            Longitude: 29.060,
            RadiusMeters: 5_000,
            ReportType: null,
            Species: null,
            Limit: 20);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public async Task Validate_ShouldFail_WhenLatitudeIsInvalid(
        double latitude)
    {
        var query = CreateValidQuery() with
        {
            Latitude = latitude
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(x => x.PropertyName == nameof(query.Latitude));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public async Task Validate_ShouldFail_WhenLongitudeIsInvalid(
        double longitude)
    {
        var query = CreateValidQuery() with
        {
            Longitude = longitude
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(x => x.PropertyName == nameof(query.Longitude));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(50001)]
    public async Task Validate_ShouldFail_WhenRadiusIsInvalid(
        double radiusMeters)
    {
        var query = CreateValidQuery() with
        {
            RadiusMeters = radiusMeters
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(x => x.PropertyName == nameof(query.RadiusMeters));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validate_ShouldFail_WhenLimitIsInvalid(int limit)
    {
        var query = CreateValidQuery() with
        {
            Limit = limit
        };

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(x => x.PropertyName == nameof(query.Limit));
    }

    private static GetNearbyPetReportsQuery CreateValidQuery()
    {
        return new GetNearbyPetReportsQuery(
            Latitude: 40.195,
            Longitude: 29.060,
            RadiusMeters: 5_000,
            ReportType: null,
            Species: null,
            Limit: 20);
    }
}