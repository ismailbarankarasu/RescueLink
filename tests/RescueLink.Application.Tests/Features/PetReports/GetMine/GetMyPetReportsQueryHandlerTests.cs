using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.GetMine;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests
    .Features.PetReports.GetMine;

public sealed class GetMyPetReportsQueryHandlerTests
{
    private readonly Mock<IMyPetReportReadService>
        _readServiceMock = new();

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnUserReports_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();

        var query = new GetMyPetReportsQuery(
            Page: 2,
            PageSize: 10,
            ReportType: ReportType.Lost,
            Status: ReportStatus.Active);

        var item = new MyPetReportListItemResponse(
            Id: Guid.NewGuid(),
            ReportType: ReportType.Lost,
            Status: ReportStatus.Active,
            Title: "Kayıp tekir kedi",
            Species: AnimalSpecies.Cat,
            Gender: AnimalGender.Male,
            PetName: "Pamuk",
            Breed: "Tekir",
            PrimaryColor: AnimalColor.White,
            SecondaryColor: AnimalColor.Gray,
            EventDate: DateTimeOffset.UtcNow.AddHours(-2),
            Latitude: 40.2235,
            Longitude: 28.9730,
            CreatedAt: DateTimeOffset.UtcNow.AddHours(-1),
            UpdatedAt: null,
            PrimaryPhotoStorageKey:
                "uploads/pet-reports/test.webp");

        var pagedResult =
            new PagedResult<MyPetReportListItemResponse>(
                Items: [item],
                Page: 2,
                PageSize: 10,
                TotalCount: 11);

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _readServiceMock
            .Setup(x => x.GetAsync(
                userId,
                query.Page,
                query.PageSize,
                query.ReportType,
                query.Status,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var handler = CreateHandler();

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pagedResult);

        _readServiceMock.Verify(
            x => x.GetAsync(
                userId,
                2,
                10,
                ReportType.Lost,
                ReportStatus.Active,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsUnauthenticated()
    {
        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(
            new GetMyPetReportsQuery(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(
            PetReportErrors.Unauthenticated.Code);

        _readServiceMock.Verify(
            x => x.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ReportType?>(),
                It.IsAny<ReportStatus?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetMyPetReportsQueryHandler CreateHandler()
    {
        return new GetMyPetReportsQueryHandler(
            _readServiceMock.Object,
            _currentUserServiceMock.Object);
    }
}