using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Features.PetReports.Nearby;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests.Features.PetReports.Nearby;

public sealed class GetNearbyPetReportsQueryHandlerTests
{
    private readonly Mock<IPetReportReadService> _readServiceMock;
    private readonly GetNearbyPetReportsQueryHandler _handler;

    public GetNearbyPetReportsQueryHandlerTests()
    {
        _readServiceMock = new Mock<IPetReportReadService>();
        _handler = new GetNearbyPetReportsQueryHandler(
            _readServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNearbyReports()
    {
        var query = new GetNearbyPetReportsQuery(
            Latitude: 40.195,
            Longitude: 29.060,
            RadiusMeters: 5_000,
            ReportType: ReportType.Lost,
            Species: AnimalSpecies.Cat,
            Limit: 20);

        IReadOnlyCollection<NearbyPetReportResponse> reports =
        [
            new NearbyPetReportResponse
            {
                Id = Guid.NewGuid(),
                ReportType = "Lost",
                Status = "Active",
                Title = "Kayıp kedi",
                Species = "Cat",
                PrimaryColor = "Gray",
                EventDate = DateTimeOffset.UtcNow.AddDays(-1),
                Latitude = 40.195,
                Longitude = 29.060,
                DistanceMeters = 250
            }
        ];

        _readServiceMock
            .Setup(x => x.GetNearbyAsync(
                query.Latitude,
                query.Longitude,
                query.RadiusMeters,
                query.ReportType,
                query.Species,
                query.Limit,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(reports);

        _readServiceMock.Verify(x => x.GetNearbyAsync(
                query.Latitude,
                query.Longitude,
                query.RadiusMeters,
                query.ReportType,
                query.Species,
                query.Limit,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}