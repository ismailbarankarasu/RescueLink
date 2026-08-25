using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports.GetMine;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class MyPetReportReadService(
    IDbConnectionFactory connectionFactory)
    : IMyPetReportReadService
{
    public async Task<PagedResult<MyPetReportListItemResponse>>
        GetAsync(
            Guid userId,
            int page,
            int pageSize,
            ReportType? reportType,
            ReportStatus? status,
            bool archivedOnly,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
    SELECT COUNT_BIG(1)
    FROM dbo.PetReports AS report
    WHERE report.UserId = @UserId
      AND report.IsArchived = @IsArchived
      AND (
            @ReportType IS NULL
            OR report.ReportType = @ReportType
          )
      AND (
            @Status IS NULL
            OR report.Status = @Status
          );

    SELECT
        report.Id,
        report.ReportType,
        report.Status,
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
        report.UpdatedAt,
        report.CreatedAt,
        report.UpdatedAt,
        report.IsArchived,
        report.ArchivedAt,
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
    WHERE report.UserId = @UserId
      AND report.IsArchived = @IsArchived
      AND (
            @ReportType IS NULL
            OR report.ReportType = @ReportType
          )
      AND (
            @Status IS NULL
            OR report.Status = @Status
          )
    ORDER BY report.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    """;

        var parameters = new
        {
            UserId = userId,
            IsArchived = archivedOnly,

            ReportType = reportType.HasValue
                ? (int?)reportType.Value
                : null,

            Status = status.HasValue
                ? (int?)status.Value
                : null,

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
            (await result.ReadAsync<MyPetReportListItemRow>())
            .ToArray();

        var items = rows
            .Select(row => new MyPetReportListItemResponse(
                Id: row.Id,
                ReportType: row.ReportType,
                Status: row.Status,
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
                UpdatedAt: row.UpdatedAt,
                PrimaryPhotoStorageKey:
                    row.PrimaryPhotoStorageKey,
                IsArchived: row.IsArchived,
                ArchivedAt: row.ArchivedAt))
            .ToArray();

        return new PagedResult<MyPetReportListItemResponse>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: checked((int)totalCountLong));
    }

    private sealed class MyPetReportListItemRow
    {
        public Guid Id { get; init; }
        public ReportType ReportType { get; init; }
        public ReportStatus Status { get; init; }
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
        public DateTimeOffset? UpdatedAt { get; init; }
        public string? PrimaryPhotoStorageKey { get; init; }
        public bool IsArchived { get; init; }

        public DateTimeOffset? ArchivedAt { get; init; }
    }
}