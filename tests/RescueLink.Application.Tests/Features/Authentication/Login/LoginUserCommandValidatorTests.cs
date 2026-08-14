using FluentValidation.TestHelper;
using RescueLink.Application.Features.Authentication.Login;

namespace RescueLink.Application.Tests.Features.Authentication.Login;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotContainErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = new LoginUserCommand(
            "ismail@example.com",
            "Password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Validate_ShouldContainError_WhenEmailIsInvalid(
        string email)
    {
        // Arrange
        var command = new LoginUserCommand(
            email,
            "Password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldContainError_WhenPasswordIsEmpty(
        string password)
    {
        // Arrange
        var command = new LoginUserCommand(
            "ismail@example.com",
            password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Password);
    }
}