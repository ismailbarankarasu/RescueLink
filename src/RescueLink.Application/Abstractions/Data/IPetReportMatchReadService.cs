using RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;

namespace RescueLink.Application.Abstractions.Data;

public interface IPetReportMatchReadService
{
    Task<IReadOnlyCollection<PetReportMatchResponse>>
        GetByReportIdAsync(
            Guid petReportId,
            CancellationToken cancellationToken = default);
}