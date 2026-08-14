using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Common;
using RescueLink.Application.Features.Authentication.Login;

namespace RescueLink.Application.Tests.Features.Authentication.Login;

public class LoginUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAuthenticationResponse_WhenLoginSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var expectedResponse = new AuthenticationResponse(
            UserId: userId,
            FirstName: "İsmail",
            LastName: "Karasu",
            Email: "ismail@example.com",
            AccessToken: "generated-jwt-token",
            ExpiresAt: expiresAt);

        var identityServiceMock =
            new Mock<IIdentityService>();

        identityServiceMock
            .Setup(service => service.LoginAsync(
                "ismail@example.com",
                "Password123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(expectedResponse));

        var handler = new LoginUserCommandHandler(
            identityServiceMock.Object);

        var command = new LoginUserCommand(
            Email: "  ismail@example.com  ",
            Password: "Password123");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);

        identityServiceMock.Verify(
            service => service.LoginAsync(
                "ismail@example.com",
                "Password123",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var identityServiceMock =
            new Mock<IIdentityService>();

        identityServiceMock
            .Setup(service => service.LoginAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<AuthenticationResponse>(
                    AuthenticationErrors.InvalidCredentials));

        var handler = new LoginUserCommandHandler(
            identityServiceMock.Object);

        var command = new LoginUserCommand(
            "ismail@example.com",
            "WrongPassword");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            AuthenticationErrors.InvalidCredentials);
    }
}