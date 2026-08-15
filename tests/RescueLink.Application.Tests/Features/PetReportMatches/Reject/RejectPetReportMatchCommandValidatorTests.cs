using FluentAssertions;
using RescueLink.Application.Features.PetReportMatches.Reject;

namespace RescueLink.Application.Tests
    .Features.PetReportMatches.Reject;

public sealed class RejectPetReportMatchCommandValidatorTests
{
    private readonly RejectPetReportMatchCommandValidator _validator =
        new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenMatchIdIsValid()
    {
        var command = new RejectPetReportMatchCommand(
            Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenMatchIdIsEmpty()
    {
        var command = new RejectPetReportMatchCommand(
            Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }
}