using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Abstractions.Persistence;

public interface IPetReportMatchRepository
{
    Task AddRangeAsync(
        IReadOnlyCollection<PetReportMatch> matches,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<Guid>> GetExistingCounterpartIdsAsync(
        Guid sourceReportId,
        ReportType sourceReportType,
        IReadOnlyCollection<Guid> candidateReportIds,
        CancellationToken cancellationToken = default);
}