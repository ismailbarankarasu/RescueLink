using FluentAssertions;
using RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;

namespace RescueLink.Application.Tests.Features.PetReports
    .Matching.GetByReportId;

public sealed class GetPetReportMatchesQueryValidatorTests
{
    private readonly GetPetReportMatchesQueryValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenIdIsValid()
    {
        var query = new GetPetReportMatchesQuery(
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenIdIsEmpty()
    {
        var query = new GetPetReportMatchesQuery(
            Guid.Empty);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }
}