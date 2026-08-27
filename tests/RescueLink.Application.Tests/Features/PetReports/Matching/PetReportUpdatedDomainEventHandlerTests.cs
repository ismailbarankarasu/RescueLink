using MediatR;
using Moq;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.PetReports.Matching;
using RescueLink.Application.Features.PetReports
    .Matching.Recalculate;
using RescueLink.Domain.Events;

namespace RescueLink.Application.Tests
    .Features.PetReports.Matching;

public sealed class PetReportUpdatedDomainEventHandlerTests
{
    private readonly Mock<IPetReportMatchRepository>
        _matchRepositoryMock = new();

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock = new();

    private readonly Mock<ISender>
        _senderMock = new();

    [Fact]
    public async Task Handle_ShouldRemoveSuggestedMatches_SaveChangesAndRecalculate()
    {
        // Arrange
        var petReportId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var domainEvent =
            new PetReportUpdatedDomainEvent(
                petReportId);

        var notification =
            new DomainEventNotification<
                PetReportUpdatedDomainEvent>(
                domainEvent);

        _matchRepositoryMock
            .Setup(repository =>
                repository.RemoveSuggestedByReportIdAsync(
                    petReportId,
                    cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    cancellationToken))
            .ReturnsAsync(1);

        _senderMock
            .Setup(sender => sender.Send(
                It.Is<RecalculatePetReportMatchesCommand>(
                    command =>
                        command.PetReportId ==
                        petReportId),
                cancellationToken))
            .ReturnsAsync(Result.Success());

        var handler = CreateHandler();

        // Act
        await handler.Handle(
            notification,
            cancellationToken);

        // Assert
        _matchRepositoryMock.Verify(
            repository =>
                repository.RemoveSuggestedByReportIdAsync(
                    petReportId,
                    cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    cancellationToken),
            Times.Once);

        _senderMock.Verify(
            sender => sender.Send(
                It.Is<RecalculatePetReportMatchesCommand>(
                    command =>
                        command.PetReportId ==
                        petReportId),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldExecuteOperationsInCorrectOrder()
    {
        // Arrange
        var petReportId = Guid.NewGuid();

        var notification =
            new DomainEventNotification<
                PetReportUpdatedDomainEvent>(
                new PetReportUpdatedDomainEvent(
                    petReportId));

        var sequence = new MockSequence();

        _matchRepositoryMock
            .InSequence(sequence)
            .Setup(repository =>
                repository.RemoveSuggestedByReportIdAsync(
                    petReportId,
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .InSequence(sequence)
            .Setup(unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _senderMock
            .InSequence(sequence)
            .Setup(sender => sender.Send(
                It.Is<RecalculatePetReportMatchesCommand>(
                    command =>
                        command.PetReportId ==
                        petReportId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = CreateHandler();

        // Act
        await handler.Handle(
            notification,
            CancellationToken.None);

        // Assert
        _matchRepositoryMock.VerifyAll();
        _unitOfWorkMock.VerifyAll();
        _senderMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_ShouldNotRecalculate_WhenRemovingMatchesFails()
    {
        // Arrange
        var petReportId = Guid.NewGuid();

        var notification =
            new DomainEventNotification<
                PetReportUpdatedDomainEvent>(
                new PetReportUpdatedDomainEvent(
                    petReportId));

        _matchRepositoryMock
            .Setup(repository =>
                repository.RemoveSuggestedByReportIdAsync(
                    petReportId,
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Database operation failed."));

        var handler = CreateHandler();

        // Act
        var action = async () =>
            await handler.Handle(
                notification,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<
            InvalidOperationException>(action);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _senderMock.Verify(
            sender => sender.Send(
                It.IsAny<
                    RecalculatePetReportMatchesCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotRecalculate_WhenSavingChangesFails()
    {
        // Arrange
        var petReportId = Guid.NewGuid();

        var notification =
            new DomainEventNotification<
                PetReportUpdatedDomainEvent>(
                new PetReportUpdatedDomainEvent(
                    petReportId));

        _matchRepositoryMock
            .Setup(repository =>
                repository.RemoveSuggestedByReportIdAsync(
                    petReportId,
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Save operation failed."));

        var handler = CreateHandler();

        // Act
        var action = async () =>
            await handler.Handle(
                notification,
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<
            InvalidOperationException>(action);

        _senderMock.Verify(
            sender => sender.Send(
                It.IsAny<
                    RecalculatePetReportMatchesCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PetReportUpdatedDomainEventHandler
        CreateHandler()
    {
        return new PetReportUpdatedDomainEventHandler(
            _matchRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _senderMock.Object);
    }
}