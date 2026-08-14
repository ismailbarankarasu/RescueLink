using FluentValidation.TestHelper;
using RescueLink.Application.Features.Authentication.Register;

namespace RescueLink.Application.Tests.Features.Authentication.Register;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotContainErrors_WhenCommandIsValid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Validate_ShouldContainError_WhenEmailIsInvalid(
        string email)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Email = email
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Email);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("password1")]
    [InlineData("PASSWORD1")]
    [InlineData("Password")]
    public void Validate_ShouldContainError_WhenPasswordIsInvalid(
        string password)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.Password);
    }

    [Fact]
    public void Validate_ShouldContainError_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Password = "Password123",
            ConfirmPassword = "Different123"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            value => value.ConfirmPassword);
    }

    private static RegisterUserCommand CreateValidCommand()
    {
        return new RegisterUserCommand(
            FirstName: "İsmail",
            LastName: "Karasu",
            Email: "ismail@example.com",
            Password: "Password123",
            ConfirmPassword: "Password123");
    }
}