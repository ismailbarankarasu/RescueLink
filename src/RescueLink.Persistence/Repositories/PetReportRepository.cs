using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Domain.Entities;
using RescueLink.Persistence.Context;

namespace RescueLink.Persistence.Repositories;

public sealed class PetReportRepository
    : IPetReportRepository
{
    private readonly RescueLinkDbContext _dbContext;

    public PetReportRepository(
        RescueLinkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        PetReport petReport,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PetReports.AddAsync(
            petReport,
            cancellationToken);
    }

    public async Task<PetReport?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PetReports
            .Include(report => report.Photos)
            .SingleOrDefaultAsync(
                report => report.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PetReports
            .AsNoTracking()
            .AnyAsync(
                report => report.Id == id,
                cancellationToken);
    }

    public async Task<PetReport?> GetByIdReadOnlyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PetReports
            .AsNoTracking()
            .Include(report => report.Photos)
            .SingleOrDefaultAsync(
                report => report.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<PetReport>>
    GetByIdsReadOnlyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.PetReports
            .AsNoTracking()
            .Where(report => ids.Contains(report.Id))
            .ToArrayAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<PetReport>> GetByIdsAsync(
    IReadOnlyCollection<Guid> ids,
    CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.PetReports
            .Where(report => ids.Contains(report.Id))
            .ToArrayAsync(cancellationToken);
    }
}