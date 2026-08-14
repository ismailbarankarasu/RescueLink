using FluentAssertions;
using RescueLink.Application.Features.PetReports.Photos.SetPrimary;

namespace RescueLink.Application.Tests
    .Features.PetReports.Photos.SetPrimary;

public sealed class SetPrimaryPetReportPhotoCommandValidatorTests
{
    private readonly SetPrimaryPetReportPhotoCommandValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenIdsAreValid()
    {
        var command = new SetPrimaryPetReportPhotoCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenIdsAreEmpty()
    {
        var command = new SetPrimaryPetReportPhotoCommand(
            Guid.Empty,
            Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}