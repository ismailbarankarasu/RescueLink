using FluentAssertions;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Tests.Common.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error(
            "PetReport.NotFound",
            "Pet report was not found.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessOfT_ShouldContainProvidedValue()
    {
        // Arrange
        var reportId = Guid.NewGuid();

        // Act
        var result = Result.Success(reportId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(reportId);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void FailureOfT_ShouldContainError()
    {
        // Arrange
        var error = new Error(
            "PetReport.NotFound",
            "Pet report was not found.");

        // Act
        var result = Result.Failure<Guid>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_ShouldThrowInvalidOperationException_WhenResultIsFailure()
    {
        // Arrange
        var error = new Error(
            "PetReport.NotFound",
            "Pet report was not found.");

        var result = Result.Failure<Guid>(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "The value of a failed result cannot be accessed.");
    }
}