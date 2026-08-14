using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication.Register;

namespace RescueLink.Application.Tests.Features.Authentication.Register;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRegisterUserWithNormalizedValues()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var identityServiceMock =
            new Mock<IIdentityService>();

        identityServiceMock
            .Setup(service => service.RegisterAsync(
                "İsmail",
                "Karasu",
                "ismail@example.com",
                "Password123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userId));

        var handler = new RegisterUserCommandHandler(
            identityServiceMock.Object);

        var command = new RegisterUserCommand(
            FirstName: "  İsmail  ",
            LastName: "  Karasu  ",
            Email: "  ismail@example.com  ",
            Password: "Password123",
            ConfirmPassword: "Password123");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(userId);

        identityServiceMock.Verify(
            service => service.RegisterAsync(
                "İsmail",
                "Karasu",
                "ismail@example.com",
                "Password123",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}