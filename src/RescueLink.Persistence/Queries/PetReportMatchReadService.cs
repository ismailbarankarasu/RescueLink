using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class PetReportMatchReadService(
    IDbConnectionFactory connectionFactory)
    : IPetReportMatchReadService
{
    public async Task<IReadOnlyCollection<PetReportMatchResponse>>
        GetByReportIdAsync(
            Guid petReportId,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                match.Id AS MatchId,
                counterpart.Id AS CounterpartReportId,
                counterpart.ReportType,
                counterpart.Title,
                counterpart.Species,
                counterpart.Gender,
                counterpart.Breed,
                counterpart.PrimaryColor,
                counterpart.SecondaryColor,
                counterpart.EventDate,
                counterpart.Location.Lat AS Latitude,
                counterpart.Location.Long AS Longitude,
                match.Score,
                match.DistanceMeters,
                match.Status,
                primaryPhoto.StorageKey
                    AS PrimaryPhotoStorageKey
            FROM dbo.PetReportMatches AS match
            INNER JOIN dbo.PetReports AS counterpart
                ON counterpart.Id =
                    CASE
                        WHEN match.LostReportId = @PetReportId
                            THEN match.FoundReportId
                        ELSE match.LostReportId
                    END
            OUTER APPLY
            (
                SELECT TOP (1)
                    photo.StorageKey
                FROM dbo.PetReportPhotos AS photo
                WHERE photo.PetReportId = counterpart.Id
                ORDER BY
                    photo.IsPrimary DESC,
                    photo.DisplayOrder ASC
            ) AS primaryPhoto
            WHERE
                (
                    match.LostReportId = @PetReportId
                    OR match.FoundReportId = @PetReportId
                )
                AND match.Status IN
                    (@SuggestedStatus, @ConfirmedStatus)
            ORDER BY
                match.Score DESC,
                match.DistanceMeters ASC;
            """;

        var parameters = new
        {
            PetReportId = petReportId,
            SuggestedStatus = (int)MatchStatus.Suggested,
            ConfirmedStatus = (int)MatchStatus.Confirmed
        };

        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var rows =
            await connection.QueryAsync<PetReportMatchRow>(
            command);

        return rows
            .Select(row => new PetReportMatchResponse(
                MatchId: row.MatchId,
                CounterpartReportId: row.CounterpartReportId,
                ReportType: row.ReportType,
                Title: row.Title,
                Species: row.Species,
                Gender: row.Gender,
                Breed: row.Breed,
                PrimaryColor: row.PrimaryColor,
                SecondaryColor: row.SecondaryColor,
                EventDate: row.EventDate,
                Latitude: row.Latitude,
                Longitude: row.Longitude,
                Score: row.Score,
                DistanceMeters: row.DistanceMeters,
                Status: row.Status,
                PrimaryPhotoStorageKey:
                    row.PrimaryPhotoStorageKey))
            .ToArray();
    }
    private sealed class PetReportMatchRow
    {
        public Guid MatchId { get; init; }
        public Guid CounterpartReportId { get; init; }
        public ReportType ReportType { get; init; }
        public string Title { get; init; } = string.Empty;
        public AnimalSpecies Species { get; init; }
        public AnimalGender Gender { get; init; }
        public string? Breed { get; init; }
        public AnimalColor PrimaryColor { get; init; }
        public AnimalColor? SecondaryColor { get; init; }
        public DateTimeOffset EventDate { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public int Score { get; init; }
        public double DistanceMeters { get; init; }
        public MatchStatus Status { get; init; }
        public string? PrimaryPhotoStorageKey { get; init; }
    }
}