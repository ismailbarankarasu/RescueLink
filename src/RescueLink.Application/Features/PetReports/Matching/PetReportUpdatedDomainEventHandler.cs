using MediatR;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Application.Features.PetReports
    .Matching.Recalculate;
using RescueLink.Domain.Events;

namespace RescueLink.Application.Features.PetReports.Matching;

public sealed class PetReportUpdatedDomainEventHandler
    : INotificationHandler<
        DomainEventNotification<PetReportUpdatedDomainEvent>>
{
    private readonly IPetReportMatchRepository
        _matchRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly ISender
        _sender;

    public PetReportUpdatedDomainEventHandler(
        IPetReportMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ISender sender)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task Handle(
        DomainEventNotification<PetReportUpdatedDomainEvent>
            notification,
        CancellationToken cancellationToken)
    {
        var petReportId =
            notification.DomainEvent.PetReportId;

        await _matchRepository
            .RemoveSuggestedByReportIdAsync(
                petReportId,
                cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _sender.Send(
            new RecalculatePetReportMatchesCommand(
                petReportId),
            cancellationToken);
    }
}