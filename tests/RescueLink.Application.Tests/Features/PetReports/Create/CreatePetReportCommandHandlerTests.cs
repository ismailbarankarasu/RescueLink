using FluentAssertions;
using Moq;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.Create;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Tests.Features.PetReports.Create;

public class CreatePetReportCommandHandlerTests
{
    private readonly Mock<IPetReportRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public CreatePetReportCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPetReportRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_ShouldCreatePetReport_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = CreateValidCommand();
        PetReport? capturedReport = null;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(userId);

        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<PetReport>(),
                It.IsAny<CancellationToken>()))
            .Callback<PetReport, CancellationToken>(
                (report, _) => capturedReport = report)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        capturedReport.Should().NotBeNull();
        capturedReport!.Id.Should().Be(result.Value);
        capturedReport.UserId.Should().Be(userId);
        capturedReport.Title.Should().Be(command.Title);
        capturedReport.Location.Latitude.Should()
            .Be(command.Latitude);
        capturedReport.Location.Longitude.Should()
            .Be(command.Longitude);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PetReport>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsUnauthenticated()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();
        var command = CreateValidCommand();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            PetReportErrors.Unauthenticated);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PetReport>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CreatePetReportCommandHandler CreateHandler()
    {
        return new CreatePetReportCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    private static CreatePetReportCommand CreateValidCommand()
    {
        return new CreatePetReportCommand(
            ReportType: ReportType.Lost,
            Title: "Kayıp kedi",
            Description: "Bursa Nilüfer bölgesinde kayboldu.",
            Species: AnimalSpecies.Cat,
            Gender: AnimalGender.Female,
            PetName: "Luna",
            Breed: "Tekir",
            PrimaryColor: AnimalColor.Gray,
            SecondaryColor: AnimalColor.White,
            EventDate: DateTimeOffset.UtcNow.AddHours(-1),
            Latitude: 40.195,
            Longitude: 29.060);
    }
}