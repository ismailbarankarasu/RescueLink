using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Features.Users;
using RescueLink.Application.Features.Users.GetCurrent;

namespace RescueLink.Application.Tests
    .Features.Users.GetCurrent;

public sealed class GetCurrentUserProfileQueryHandlerTests
{
    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    private readonly Mock<IIdentityService>
        _identityServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var profile = new UserProfileInfo(
            UserId: userId,
            FirstName: "İsmail",
            LastName: "Karasu",
            Email: "ismail@example.com",
            PhoneNumber: "+905551234567",
            CountryCode: "TR",
            City: "Bursa",
            PreferredLanguage: "tr",
            TimeZoneId: "Europe/Istanbul",
            CreatedAt: DateTimeOffset.UtcNow);

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _identityServiceMock
            .Setup(service =>
                service.GetUserProfileAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var handler = CreateHandler();

        var query =
            new GetCurrentUserProfileQuery();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.UserId.Should().Be(userId);
        result.Value.FirstName.Should().Be("İsmail");
        result.Value.LastName.Should().Be("Karasu");
        result.Value.Email.Should().Be(
            "ismail@example.com");

        result.Value.PhoneNumber.Should().Be(
            "+905551234567");

        result.Value.CountryCode.Should().Be("TR");
        result.Value.City.Should().Be("Bursa");

        result.Value.PreferredLanguage.Should().Be(
            "tr");

        result.Value.TimeZoneId.Should().Be(
            "Europe/Istanbul");
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
            new GetCurrentUserProfileQuery(),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            UserProfileErrors.Unauthenticated);

        _identityServiceMock.Verify(
            service => service.GetUserProfileAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProfileDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _identityServiceMock
            .Setup(service =>
                service.GetUserProfileAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileInfo?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetCurrentUserProfileQuery(),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(
            UserProfileErrors.NotFound);
    }

    private GetCurrentUserProfileQueryHandler CreateHandler()
    {
        return new GetCurrentUserProfileQueryHandler(
            _currentUserServiceMock.Object,
            _identityServiceMock.Object);
    }
}