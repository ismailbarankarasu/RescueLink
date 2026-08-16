using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports.GetList;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class PetReportListReadService(
    IDbConnectionFactory connectionFactory)
    : IPetReportListReadService
{
    public async Task<PagedResult<PetReportListItemResponse>>
        GetListAsync(
            int page,
            int pageSize,
            ReportType? reportType,
            AnimalSpecies? species,
            string? search,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT_BIG(1)
            FROM dbo.PetReports AS report
            WHERE report.Status = @ActiveStatus
              AND (
                    @ReportType IS NULL
                    OR report.ReportType = @ReportType
                  )
              AND (
                    @Species IS NULL
                    OR report.Species = @Species
                  )
              AND (
                    @SearchPattern IS NULL
                    OR report.Title LIKE @SearchPattern
                    OR report.PetName LIKE @SearchPattern
                    OR report.Breed LIKE @SearchPattern
                  );

            SELECT
                report.Id,
                report.ReportType,
                report.Title,
                report.Species,
                report.Gender,
                report.PetName,
                report.Breed,
                report.PrimaryColor,
                report.SecondaryColor,
                report.EventDate,
                report.Location.Lat AS Latitude,
                report.Location.Long AS Longitude,
                report.CreatedAt,
                primaryPhoto.StorageKey
                    AS PrimaryPhotoStorageKey
            FROM dbo.PetReports AS report
            OUTER APPLY
            (
                SELECT TOP (1)
                    photo.StorageKey
                FROM dbo.PetReportPhotos AS photo
                WHERE photo.PetReportId = report.Id
                ORDER BY
                    photo.IsPrimary DESC,
                    photo.DisplayOrder ASC
            ) AS primaryPhoto
            WHERE report.Status = @ActiveStatus
              AND (
                    @ReportType IS NULL
                    OR report.ReportType = @ReportType
                  )
              AND (
                    @Species IS NULL
                    OR report.Species = @Species
                  )
              AND (
                    @SearchPattern IS NULL
                    OR report.Title LIKE @SearchPattern
                    OR report.PetName LIKE @SearchPattern
                    OR report.Breed LIKE @SearchPattern
                  )
            ORDER BY report.CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            """;

        var normalizedSearch =
            string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

        var parameters = new
        {
            ActiveStatus = (int)ReportStatus.Active,

            ReportType = reportType.HasValue
                ? (int?)reportType.Value
                : null,

            Species = species.HasValue
                ? (int?)species.Value
                : null,

            SearchPattern = normalizedSearch is null
                ? null
                : $"%{normalizedSearch}%",

            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        using var result =
            await connection.QueryMultipleAsync(command);

        var totalCountLong =
            await result.ReadSingleAsync<long>();

        var rows =
            (await result.ReadAsync<PetReportListItemRow>())
            .ToArray();

        var items = rows
            .Select(row => new PetReportListItemResponse(
                Id: row.Id,
                ReportType: row.ReportType,
                Title: row.Title,
                Species: row.Species,
                Gender: row.Gender,
                PetName: row.PetName,
                Breed: row.Breed,
                PrimaryColor: row.PrimaryColor,
                SecondaryColor: row.SecondaryColor,
                EventDate: row.EventDate,
                Latitude: row.Latitude,
                Longitude: row.Longitude,
                CreatedAt: row.CreatedAt,
                PrimaryPhotoStorageKey:
                    row.PrimaryPhotoStorageKey))
            .ToArray();

        return new PagedResult<PetReportListItemResponse>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: checked((int)totalCountLong));
    }

    private sealed class PetReportListItemRow
    {
        public Guid Id { get; init; }
        public ReportType ReportType { get; init; }
        public string Title { get; init; } = string.Empty;
        public AnimalSpecies Species { get; init; }
        public AnimalGender Gender { get; init; }
        public string? PetName { get; init; }
        public string? Breed { get; init; }
        public AnimalColor PrimaryColor { get; init; }
        public AnimalColor? SecondaryColor { get; init; }
        public DateTimeOffset EventDate { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public string? PrimaryPhotoStorageKey { get; init; }
    }
}