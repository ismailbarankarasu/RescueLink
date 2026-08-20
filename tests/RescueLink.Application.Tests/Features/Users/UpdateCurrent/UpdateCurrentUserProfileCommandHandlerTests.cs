using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Users;
using RescueLink.Application.Features.Users.UpdateCurrent;

namespace RescueLink.Application.Tests
    .Features.Users.UpdateCurrent;

public sealed class UpdateCurrentUserProfileCommandHandlerTests
{
    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    private readonly Mock<IIdentityService>
        _identityServiceMock = new();

    [Fact]
    public async Task Handle_ShouldUpdateProfile_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _identityServiceMock
            .Setup(service =>
                service.UpdateUserProfileAsync(
                    userId,
                    It.Is<UpdateUserProfileInfo>(
                        profile =>
                            profile.FirstName == "İsmail" &&
                            profile.LastName == "Karasu" &&
                            profile.PhoneNumber ==
                                "+905551234567" &&
                            profile.CountryCode == "TR" &&
                            profile.City == "Bursa" &&
                            profile.PreferredLanguage ==
                                "tr" &&
                            profile.TimeZoneId ==
                                "Europe/Istanbul"),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = CreateHandler();

        var command =
            CreateValidCommand();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _identityServiceMock.Verify(
            service => service.UpdateUserProfileAsync(
                userId,
                It.IsAny<UpdateUserProfileInfo>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            CreateValidCommand(),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            UserProfileErrors.Unauthenticated);

        _identityServiceMock.Verify(
            service => service.UpdateUserProfileAsync(
                It.IsAny<Guid>(),
                It.IsAny<UpdateUserProfileInfo>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
     
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenIdentityUpdateFails()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _identityServiceMock
            .Setup(service =>
                service.UpdateUserProfileAsync(
                    userId,
                    It.IsAny<UpdateUserProfileInfo>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure(
                    UserProfileErrors.NotFound));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            CreateValidCommand(),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            UserProfileErrors.NotFound);
    }

    private UpdateCurrentUserProfileCommandHandler
        CreateHandler()
    {
        return new UpdateCurrentUserProfileCommandHandler(
            _currentUserServiceMock.Object,
            _identityServiceMock.Object);
    }

    private static UpdateCurrentUserProfileCommand
        CreateValidCommand()
    {
        return new UpdateCurrentUserProfileCommand(
            FirstName: "İsmail",
            LastName: "Karasu",
            PhoneNumber: "+905551234567",
            CountryCode: "TR",
            City: "Bursa",
            PreferredLanguage: "tr",
            TimeZoneId: "Europe/Istanbul");
    }
}