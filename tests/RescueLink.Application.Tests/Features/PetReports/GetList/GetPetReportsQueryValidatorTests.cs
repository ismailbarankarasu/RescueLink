using FluentAssertions;
using RescueLink.Application.Features.PetReports.GetList;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests.Features.PetReports.GetList;

public sealed class GetPetReportsQueryValidatorTests
{
    private readonly GetPetReportsQueryValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenQueryIsValid()
    {
        var query = new GetPetReportsQuery(
            Page: 1,
            PageSize: 12,
            ReportType: ReportType.Lost,
            Species: AnimalSpecies.Cat,
            Search: "Tekir");

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_ShouldFail_WhenPageIsInvalid(
        int page)
    {
        var query = new GetPetReportsQuery(
            Page: page);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            error => error.PropertyName ==
                     nameof(query.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task Validate_ShouldFail_WhenPageSizeIsInvalid(
        int pageSize)
    {
        var query = new GetPetReportsQuery(
            PageSize: pageSize);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            error => error.PropertyName ==
                     nameof(query.PageSize));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSearchIsTooLong()
    {
        var query = new GetPetReportsQuery(
            Search: new string('a', 101));

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            error => error.PropertyName ==
                     nameof(query.Search));
    }

    [Fact]
    public async Task Validate_ShouldSucceed_WhenFiltersAreEmpty()
    {
        var query = new GetPetReportsQuery();

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }
}