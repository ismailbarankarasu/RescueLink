using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Common;
using RescueLink.Application.Features.Authentication.Refresh;

namespace RescueLink.Application.Tests.Features
    .Authentication.Refresh;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IIdentityService>
        _identityServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var accessTokenExpiresAt =
            DateTimeOffset.UtcNow.AddHours(1);

        var refreshTokenExpiresAt =
            DateTimeOffset.UtcNow.AddDays(7);

        var expectedResponse =
            new AuthenticationResponse(
                UserId: Guid.NewGuid(),
                FirstName: "İsmail",
                LastName: "Karasu",
                Email: "ismail@example.com",
                AccessToken: "new-access-token",
                ExpiresAt: accessTokenExpiresAt,
                RefreshToken: "new-refresh-token",
                RefreshTokenExpiresAt:
                    refreshTokenExpiresAt);

        _identityServiceMock
            .Setup(service => service.RefreshAsync(
                "old-refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(expectedResponse));

        var handler = new RefreshTokenCommandHandler(
            _identityServiceMock.Object);

        var command = new RefreshTokenCommand(
            "  old-refresh-token  ");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);

        _identityServiceMock.Verify(
            service => service.RefreshAsync(
                "old-refresh-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        _identityServiceMock
            .Setup(service => service.RefreshAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<AuthenticationResponse>(
                    AuthenticationErrors
                        .InvalidRefreshToken));

        var handler = new RefreshTokenCommandHandler(
            _identityServiceMock.Object);

        var command = new RefreshTokenCommand(
            "invalid-refresh-token");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            AuthenticationErrors.InvalidRefreshToken);

        _identityServiceMock.Verify(
            service => service.RefreshAsync(
                "invalid-refresh-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}