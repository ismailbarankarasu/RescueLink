using MediatR;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;

namespace RescueLink.Application.Features.Notifications;

public sealed class PetReportMatchConfirmedNotificationHandler
    : INotificationHandler<
        DomainEventNotification<
            PetReportMatchConfirmedDomainEvent>>
{
    private readonly IPetReportRepository
        _petReportRepository;

    private readonly IUserNotificationRepository
        _notificationRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    public PetReportMatchConfirmedNotificationHandler(
        IPetReportRepository petReportRepository,
        IUserNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _petReportRepository = petReportRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DomainEventNotification<
            PetReportMatchConfirmedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent =
            notification.DomainEvent;

        var reportIds = new[]
        {
            domainEvent.LostReportId,
            domainEvent.FoundReportId
        };

        var reports =
            await _petReportRepository
                .GetByIdsReadOnlyAsync(
                    reportIds,
                    cancellationToken);

        var lostReport = reports.SingleOrDefault(
            report =>
                report.Id == domainEvent.LostReportId);

        var foundReport = reports.SingleOrDefault(
            report =>
                report.Id == domainEvent.FoundReportId);

        if (lostReport is null ||
            foundReport is null)
        {
            return;
        }

        var userIds = new[]
        {
            lostReport.UserId,
            foundReport.UserId
        }
        .Distinct()
        .ToArray();

        var notifications = userIds
            .Select(userId =>
                UserNotification.Create(
                    userId: userId,
                    type: NotificationType.MatchConfirmed,
                    title: "Eşleşme onaylandı",
                    message:
                        "Eşleşme iki tarafça onaylandı. " +
                        "İlgili ilanlar çözüldü olarak işaretlendi.",
                    relatedEntityId: domainEvent.MatchId))
            .ToArray();

        await _notificationRepository.AddRangeAsync(
            notifications,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}