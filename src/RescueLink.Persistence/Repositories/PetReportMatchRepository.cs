using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Persistence.Context;

namespace RescueLink.Persistence.Repositories;

internal sealed class PetReportMatchRepository(
    RescueLinkDbContext dbContext)
    : IPetReportMatchRepository
{
    public async Task AddRangeAsync(
        IReadOnlyCollection<PetReportMatch> matches,
        CancellationToken cancellationToken = default)
    {
        if (matches.Count == 0)
        {
            return;
        }

        await dbContext.PetReportMatches.AddRangeAsync(
            matches,
            cancellationToken);
    }

    public async Task<IReadOnlySet<Guid>>
        GetExistingCounterpartIdsAsync(
            Guid sourceReportId,
            ReportType sourceReportType,
            IReadOnlyCollection<Guid> candidateReportIds,
            CancellationToken cancellationToken = default)
    {
        if (candidateReportIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        Guid[] existingIds;

        if (sourceReportType == ReportType.Lost)
        {
            existingIds = await dbContext.PetReportMatches
                .AsNoTracking()
                .Where(match =>
                    match.LostReportId == sourceReportId &&
                    candidateReportIds.Contains(
                        match.FoundReportId))
                .Select(match => match.FoundReportId)
                .ToArrayAsync(cancellationToken);
        }
        else
        {
            existingIds = await dbContext.PetReportMatches
                .AsNoTracking()
                .Where(match =>
                    match.FoundReportId == sourceReportId &&
                    candidateReportIds.Contains(
                        match.LostReportId))
                .Select(match => match.LostReportId)
                .ToArrayAsync(cancellationToken);
        }

        return existingIds.ToHashSet();
    }
    public async Task<PetReportMatch?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await dbContext.PetReportMatches
            .SingleOrDefaultAsync(
                match => match.Id == id,
                cancellationToken);
    }
    public async Task RemoveSuggestedByReportIdAsync(
    Guid petReportId,
    CancellationToken cancellationToken = default)
    {
        var suggestedMatches =
            await dbContext.PetReportMatches
                .Where(match =>
                    match.Status == MatchStatus.Suggested &&
                    (
                        match.LostReportId == petReportId ||
                        match.FoundReportId == petReportId
                    ))
                .ToArrayAsync(cancellationToken);

        if (suggestedMatches.Length == 0)
        {
            return;
        }

        dbContext.PetReportMatches.RemoveRange(
            suggestedMatches);
    }

    public async Task<PetReportMatch?> GetByIdReadOnlyAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await dbContext.PetReportMatches
            .AsNoTracking()
            .SingleOrDefaultAsync(
                match => match.Id == id,
                cancellationToken);
    }
}