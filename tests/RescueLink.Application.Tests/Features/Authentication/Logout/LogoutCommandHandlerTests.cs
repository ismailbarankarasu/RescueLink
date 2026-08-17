using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Logout;

namespace RescueLink.Application.Tests.Features
    .Authentication.Logout;

public sealed class LogoutCommandHandlerTests
{
    private readonly Mock<IIdentityService>
        _identityServiceMock = new();

    [Fact]
    public async Task Handle_ShouldLogoutUser_WhenRefreshTokenIsProvided()
    {
        // Arrange
        _identityServiceMock
            .Setup(service => service.LogoutAsync(
                "valid-refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new LogoutCommandHandler(
            _identityServiceMock.Object);

        var command = new LogoutCommand(
            "  valid-refresh-token  ");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _identityServiceMock.Verify(
            service => service.LogoutAsync(
                "valid-refresh-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIdentityServiceFails()
    {
        // Arrange
        _identityServiceMock
            .Setup(service => service.LogoutAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure(
                    AuthenticationErrors
                        .InvalidRefreshToken));

        var handler = new LogoutCommandHandler(
            _identityServiceMock.Object);

        var command = new LogoutCommand(
            "refresh-token");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            AuthenticationErrors.InvalidRefreshToken);
    }
}