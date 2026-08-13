using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using RescueLink.Application.Common.Behaviors;

namespace RescueLink.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsExist()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(
            []);

        RequestHandlerDelegate<string> next =
            _ => Task.FromResult("success");

        // Act
        var result = await behavior.Handle(
            new TestRequest("valid"),
            next,
            CancellationToken.None);

        // Assert
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();

        validatorMock
            .Setup(validator => validator.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>(
            [validatorMock.Object]);

        RequestHandlerDelegate<string> next =
            _ => Task.FromResult("success");

        // Act
        var result = await behavior.Handle(
            new TestRequest("valid"),
            next,
            CancellationToken.None);

        // Assert
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var failure = new ValidationFailure(
            nameof(TestRequest.Name),
            "Name is required.");

        var validatorMock = new Mock<IValidator<TestRequest>>();

        validatorMock
            .Setup(validator => validator.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ValidationResult([failure]));

        var behavior = new ValidationBehavior<TestRequest, string>(
            [validatorMock.Object]);

        var nextWasCalled = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            nextWasCalled = true;
            return Task.FromResult("success");
        };

        // Act
        Func<Task> act = () => behavior.Handle(
            new TestRequest(string.Empty),
            next,
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>();

        nextWasCalled.Should().BeFalse();
    }

    public sealed record TestRequest(string Name);
}