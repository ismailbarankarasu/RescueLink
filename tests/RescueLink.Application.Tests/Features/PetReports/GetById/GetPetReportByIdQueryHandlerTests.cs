using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.GetById;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Tests.Features.PetReports.GetById;

public class GetPetReportByIdQueryHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock;

    public GetPetReportByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPetReportRepository>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPetReport_WhenReportExists()
    {
        // Arrange
        var report = CreateValidReport();

        report.AddPhoto("photo-1.webp");
        report.AddPhoto("photo-2.webp");

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdReadOnlyAsync(
                    report.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var handler = new GetPetReportByIdQueryHandler(
            _repositoryMock.Object);

        var query = new GetPetReportByIdQuery(report.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var response = result.Value;

        response.Id.Should().Be(report.Id);
        response.UserId.Should().Be(report.UserId);
        response.Title.Should().Be(report.Title);
        response.Status.Should().Be(ReportStatus.Active);

        response.Latitude.Should()
            .Be(report.Location.Latitude);

        response.Longitude.Should()
            .Be(report.Location.Longitude);

        response.Photos.Should().HaveCount(2);

        response.Photos
            .Select(photo => photo.DisplayOrder)
            .Should()
            .BeInAscendingOrder();

        response.Photos
            .Count(photo => photo.IsPrimary)
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenReportDoesNotExist()
    {
        // Arrange
        var reportId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdReadOnlyAsync(
                    reportId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetReport?)null);

        var handler = new GetPetReportByIdQueryHandler(
            _repositoryMock.Object);

        var query = new GetPetReportByIdQuery(reportId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            PetReportErrors.NotFound(reportId));
    }

    private static PetReport CreateValidReport()
    {
        return PetReport.Create(
            userId: Guid.NewGuid(),
            reportType: ReportType.Lost,
            title: "Kayıp kedi",
            description: "Bursa Nilüfer bölgesinde kayboldu.",
            species: AnimalSpecies.Cat,
            gender: AnimalGender.Female,
            petName: "Luna",
            breed: "Tekir",
            primaryColor: AnimalColor.Gray,
            secondaryColor: AnimalColor.White,
            eventDate: DateTimeOffset.UtcNow.AddHours(-1),
            location: GeoLocation.Create(40.195, 29.060));
    }
}