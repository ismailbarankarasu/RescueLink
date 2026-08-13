using RescueLink.Domain.Entities;

namespace RescueLink.Application.Abstractions.Persistence;

public interface IPetReportRepository
{
    Task AddAsync(
        PetReport petReport,
        CancellationToken cancellationToken = default);

    Task<PetReport?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}