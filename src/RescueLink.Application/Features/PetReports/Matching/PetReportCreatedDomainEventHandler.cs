using MediatR;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Features.PetReports.Matching.Recalculate;
using RescueLink.Domain.Events;

namespace RescueLink.Application.Features.PetReports.Matching;

public sealed class PetReportCreatedDomainEventHandler
    : INotificationHandler<
        DomainEventNotification<PetReportCreatedDomainEvent>>
{
    private readonly ISender _sender;

    public PetReportCreatedDomainEventHandler(
        ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(
        DomainEventNotification<PetReportCreatedDomainEvent>
            notification,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new RecalculatePetReportMatchesCommand(
                notification.DomainEvent.PetReportId),
            cancellationToken);
    }
}