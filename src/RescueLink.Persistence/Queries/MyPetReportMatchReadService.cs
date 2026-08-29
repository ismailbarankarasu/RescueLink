using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReportMatches.GetMine;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class MyPetReportMatchReadService(
    IDbConnectionFactory connectionFactory)
    : IMyPetReportMatchReadService
{
    public async Task<PagedResult<MyPetReportMatchResponse>>
        GetAsync(
            Guid userId,
            int page,
            int pageSize,
            MatchStatus? status,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT_BIG(1)
            FROM dbo.PetReportMatches AS match
            INNER JOIN dbo.PetReports AS lostReport
                ON lostReport.Id = match.LostReportId
            INNER JOIN dbo.PetReports AS foundReport
                ON foundReport.Id = match.FoundReportId
            WHERE
                (
                    lostReport.UserId = @UserId
                    OR foundReport.UserId = @UserId
                )
                AND lostReport.IsArchived = 0
                AND foundReport.IsArchived = 0
                AND
                (
                    (
                        @Status IS NULL
                        AND match.Status IN
                            (@SuggestedStatus, @ConfirmedStatus)
                    )
                    OR match.Status = @Status
                );

            SELECT
                match.Id AS MatchId,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN lostReport.Id
                    ELSE foundReport.Id
                END AS SourceReportId,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN lostReport.ReportType
                    ELSE foundReport.ReportType
                END AS SourceReportType,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN lostReport.Title
                    ELSE foundReport.Title
                END AS SourceReportTitle,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Id
                    ELSE lostReport.Id
                END AS CounterpartReportId,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.ReportType
                    ELSE lostReport.ReportType
                END AS CounterpartReportType,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Title
                    ELSE lostReport.Title
                END AS CounterpartReportTitle,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Species
                    ELSE lostReport.Species
                END AS Species,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Gender
                    ELSE lostReport.Gender
                END AS Gender,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Breed
                    ELSE lostReport.Breed
                END AS Breed,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.PrimaryColor
                    ELSE lostReport.PrimaryColor
                END AS PrimaryColor,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.SecondaryColor
                    ELSE lostReport.SecondaryColor
                END AS SecondaryColor,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.EventDate
                    ELSE lostReport.EventDate
                END AS EventDate,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Location.Lat
                    ELSE lostReport.Location.Lat
                END AS Latitude,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN foundReport.Location.Long
                    ELSE lostReport.Location.Long
                END AS Longitude,

                match.Score,
                match.DistanceMeters,
                match.Status,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN match.LostOwnerConfirmed
                    ELSE match.FoundOwnerConfirmed
                END AS CurrentUserConfirmed,

                CASE
                    WHEN lostReport.UserId = @UserId
                        THEN match.FoundOwnerConfirmed
                    ELSE match.LostOwnerConfirmed
                END AS CounterpartConfirmed,

                primaryPhoto.StorageKey
                    AS PrimaryPhotoStorageKey

            FROM dbo.PetReportMatches AS match
            INNER JOIN dbo.PetReports AS lostReport
                ON lostReport.Id = match.LostReportId
            INNER JOIN dbo.PetReports AS foundReport
                ON foundReport.Id = match.FoundReportId

            OUTER APPLY
            (
                SELECT TOP (1)
                    photo.StorageKey
                FROM dbo.PetReportPhotos AS photo
                WHERE photo.PetReportId =
                    CASE
                        WHEN lostReport.UserId = @UserId
                            THEN foundReport.Id
                        ELSE lostReport.Id
                    END
                ORDER BY
                    photo.IsPrimary DESC,
                    photo.DisplayOrder ASC
            ) AS primaryPhoto

            WHERE
                (
                    lostReport.UserId = @UserId
                    OR foundReport.UserId = @UserId
                )
                AND lostReport.IsArchived = 0
                AND foundReport.IsArchived = 0
                AND
                (
                    (
                        @Status IS NULL
                        AND match.Status IN
                            (@SuggestedStatus, @ConfirmedStatus)
                    )
                    OR match.Status = @Status
                )
            ORDER BY
                match.Score DESC,
                match.DistanceMeters ASC,
                match.CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            UserId = userId,

            Status = status.HasValue
                ? (int?)status.Value
                : null,

            SuggestedStatus =
                (int)MatchStatus.Suggested,

            ConfirmedStatus =
                (int)MatchStatus.Confirmed,

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

        var items =
            (await result.ReadAsync<
                MyPetReportMatchResponse>())
            .ToArray();

        return new PagedResult<MyPetReportMatchResponse>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: checked((int)totalCountLong));
    }
}