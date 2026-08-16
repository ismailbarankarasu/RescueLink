using FluentAssertions;
using MediatR;
using Moq;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.PetReports.Matching;
using RescueLink.Application.Features.PetReports
    .Matching.Recalculate;
using RescueLink.Domain.Events;

namespace RescueLink.Application.Tests
    .Features.PetReports.Matching;

public sealed class PetReportDomainEventHandlerTests
{
    [Fact]
    public async Task CreatedHandler_ShouldRequestMatchCalculation()
    {
        var petReportId = Guid.NewGuid();
        var senderMock = new Mock<ISender>();

        senderMock
            .Setup(x => x.Send(
                It.IsAny<
                    RecalculatePetReportMatchesCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler =
            new PetReportCreatedDomainEventHandler(
                senderMock.Object);

        var notification =
            new DomainEventNotification<
                PetReportCreatedDomainEvent>(
                    new PetReportCreatedDomainEvent(
                        petReportId));

        await handler.Handle(
            notification,
            CancellationToken.None);

        senderMock.Verify(
            x => x.Send(
                It.Is<
                    RecalculatePetReportMatchesCommand>(
                        command =>
                            command.PetReportId ==
                            petReportId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatedHandler_ShouldRemoveOldSuggestions_Save_AndRecalculate()
    {
        var petReportId = Guid.NewGuid();

        var matchRepositoryMock =
            new Mock<IPetReportMatchRepository>();

        var unitOfWorkMock =
            new Mock<IUnitOfWork>();

        var senderMock =
            new Mock<ISender>();

        var callOrder = new List<string>();

        matchRepositoryMock
            .Setup(x =>
                x.RemoveSuggestedByReportIdAsync(
                    petReportId,
                    It.IsAny<CancellationToken>()))
            .Callback(() =>
                callOrder.Add("remove"))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Callback(() =>
                callOrder.Add("save"))
            .ReturnsAsync(1);

        senderMock
            .Setup(x => x.Send(
                It.IsAny<
                    RecalculatePetReportMatchesCommand>(),
                It.IsAny<CancellationToken>()))
            .Callback(() =>
                callOrder.Add("recalculate"))
            .ReturnsAsync(Result.Success());

        var handler =
            new PetReportUpdatedDomainEventHandler(
                matchRepositoryMock.Object,
                unitOfWorkMock.Object,
                senderMock.Object);

        var notification =
            new DomainEventNotification<
                PetReportUpdatedDomainEvent>(
                    new PetReportUpdatedDomainEvent(
                        petReportId));

        await handler.Handle(
            notification,
            CancellationToken.None);

        callOrder.Should().Equal(
            "remove",
            "save",
            "recalculate");

        matchRepositoryMock.Verify(
            x => x.RemoveSuggestedByReportIdAsync(
                petReportId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        senderMock.Verify(
            x => x.Send(
                It.Is<
                    RecalculatePetReportMatchesCommand>(
                        command =>
                            command.PetReportId ==
                            petReportId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}