using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports.GetList;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests.Features.PetReports.GetList;

public sealed class GetPetReportsQueryHandlerTests
{
    private readonly Mock<IPetReportListReadService>
        _readServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnPagedReports()
    {
        var query = new GetPetReportsQuery(
            Page: 2,
            PageSize: 12,
            ReportType: ReportType.Lost,
            Species: AnimalSpecies.Cat,
            Search: "Tekir");

        IReadOnlyCollection<PetReportListItemResponse> items =
        [
            new PetReportListItemResponse(
                Id: Guid.NewGuid(),
                ReportType: ReportType.Lost,
                Title: "Kayıp tekir kedi",
                Species: AnimalSpecies.Cat,
                Gender: AnimalGender.Male,
                PetName: "Atlas",
                Breed: "Tekir",
                PrimaryColor: AnimalColor.Gray,
                SecondaryColor: AnimalColor.White,
                EventDate: DateTimeOffset.UtcNow.AddDays(-1),
                Latitude: 40.2165,
                Longitude: 28.9849,
                CreatedAt: DateTimeOffset.UtcNow,
                PrimaryPhotoStorageKey: null)
        ];

        var pagedResult =
            new PagedResult<PetReportListItemResponse>(
                Items: items,
                Page: 2,
                PageSize: 12,
                TotalCount: 25);

        _readServiceMock
            .Setup(x => x.GetListAsync(
                query.Page,
                query.PageSize,
                query.ReportType,
                query.Species,
                query.Search,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var handler =
            new GetPetReportsQueryHandler(
                _readServiceMock.Object);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(pagedResult);

        result.Value.TotalPages.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();

        _readServiceMock.Verify(
            x => x.GetListAsync(
                query.Page,
                query.PageSize,
                query.ReportType,
                query.Species,
                query.Search,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}