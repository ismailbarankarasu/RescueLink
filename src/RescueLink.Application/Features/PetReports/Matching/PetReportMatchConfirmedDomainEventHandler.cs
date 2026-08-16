using MediatR;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Events;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;

namespace RescueLink.Application.Features.PetReports.Matching;

public sealed class PetReportMatchConfirmedDomainEventHandler
    : INotificationHandler<
        DomainEventNotification<
            PetReportMatchConfirmedDomainEvent>>
{
    private readonly IPetReportRepository _petReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PetReportMatchConfirmedDomainEventHandler(
        IPetReportRepository petReportRepository,
        IUnitOfWork unitOfWork)
    {
        _petReportRepository = petReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DomainEventNotification<
            PetReportMatchConfirmedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var reportIds = new[]
        {
            notification.DomainEvent.LostReportId,
            notification.DomainEvent.FoundReportId
        };

        var reports = await _petReportRepository.GetByIdsAsync(
            reportIds,
            cancellationToken);

        var activeReports = reports
            .Where(report =>
                report.Status == ReportStatus.Active)
            .ToArray();

        if (activeReports.Length == 0)
        {
            return;
        }

        foreach (var report in activeReports)
        {
            report.Resolve();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}