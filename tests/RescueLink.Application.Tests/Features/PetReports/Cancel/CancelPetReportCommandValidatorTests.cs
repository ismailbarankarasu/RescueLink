using FluentAssertions;
using RescueLink.Application.Features.PetReports.Cancel;

namespace RescueLink.Application.Tests.Features.PetReports.Cancel;

public sealed class CancelPetReportCommandValidatorTests
{
    private readonly CancelPetReportCommandValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenIdIsValid()
    {
        var command = new CancelPetReportCommand(
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenIdIsEmpty()
    {
        var command = new CancelPetReportCommand(
            Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }
}