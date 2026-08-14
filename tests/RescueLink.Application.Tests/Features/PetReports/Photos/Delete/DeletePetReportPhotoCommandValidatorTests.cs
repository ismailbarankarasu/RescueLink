using FluentAssertions;
using RescueLink.Application.Features.PetReports.Photos.Delete;

namespace RescueLink.Application.Tests
    .Features.PetReports.Photos.Delete;

public sealed class DeletePetReportPhotoCommandValidatorTests
{
    private readonly DeletePetReportPhotoCommandValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenIdsAreValid()
    {
        var command = new DeletePetReportPhotoCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenIdsAreEmpty()
    {
        var command = new DeletePetReportPhotoCommand(
            Guid.Empty,
            Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}