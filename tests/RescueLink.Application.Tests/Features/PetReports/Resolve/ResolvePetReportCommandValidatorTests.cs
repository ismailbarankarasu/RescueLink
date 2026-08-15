using FluentAssertions;
using RescueLink.Application.Features.PetReports.Resolve;

namespace RescueLink.Application.Tests.Features.PetReports.Resolve;

public sealed class ResolvePetReportCommandValidatorTests
{
    private readonly ResolvePetReportCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenIdIsValid()
    {
        var command = new ResolvePetReportCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenIdIsEmpty()
    {
        var command = new ResolvePetReportCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }
}