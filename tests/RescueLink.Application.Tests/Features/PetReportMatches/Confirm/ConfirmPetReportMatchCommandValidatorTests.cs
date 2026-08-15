using FluentAssertions;
using RescueLink.Application.Features.PetReportMatches.Confirm;

namespace RescueLink.Application.Tests
    .Features.PetReportMatches.Confirm;

public sealed class ConfirmPetReportMatchCommandValidatorTests
{
    private readonly ConfirmPetReportMatchCommandValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenMatchIdIsValid()
    {
        var command = new ConfirmPetReportMatchCommand(
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenMatchIdIsEmpty()
    {
        var command = new ConfirmPetReportMatchCommand(
            Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }
}