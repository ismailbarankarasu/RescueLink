using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features
    .PetReportMatches;
using RescueLink.Application.Features
    .PetReportMatches.GetMine;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests
    .Features.PetReportMatches.GetMine;

public sealed class GetMyPetReportMatchesQueryHandlerTests
{
    private readonly Mock<IMyPetReportMatchReadService>
        _readServiceMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnMatches_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var query =
            new GetMyPetReportMatchesQuery(
                Page: 2,
                PageSize: 10,
                Status: MatchStatus.Suggested);

        var pagedResult =
            new PagedResult<MyPetReportMatchResponse>(
                Items: [],
                Page: 2,
                PageSize: 10,
                TotalCount: 15);

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _readServiceMock
            .Setup(service => service.GetAsync(
                userId,
                query.Page,
                query.PageSize,
                query.Status,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should()
            .BeEquivalentTo(pagedResult);

        _readServiceMock.Verify(
            service => service.GetAsync(
                userId,
                2,
                10,
                MatchStatus.Suggested,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassNullStatus_WhenStatusFilterIsNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var query =
            new GetMyPetReportMatchesQuery(
                Page: 1,
                PageSize: 12,
                Status: null);

        var pagedResult =
            new PagedResult<MyPetReportMatchResponse>(
                Items: [],
                Page: 1,
                PageSize: 12,
                TotalCount: 0);

        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns(userId);

        _readServiceMock
            .Setup(service => service.GetAsync(
                userId,
                1,
                12,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _readServiceMock.Verify(
            service => service.GetAsync(
                userId,
                1,
                12,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        // Arrange
        _currentUserServiceMock
            .SetupGet(service => service.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            new GetMyPetReportMatchesQuery(),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be(
            PetReportMatchErrors
                .Unauthenticated.Code);

        _readServiceMock.Verify(
            service => service.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<MatchStatus?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetMyPetReportMatchesQueryHandler
        CreateHandler()
    {
        return new GetMyPetReportMatchesQueryHandler(
            _readServiceMock.Object,
            _currentUserServiceMock.Object);
    }
}