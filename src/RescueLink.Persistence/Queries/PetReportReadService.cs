using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Features.PetReports.Nearby;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class PetReportReadService(
    IDbConnectionFactory connectionFactory)
    : IPetReportReadService
{
    public async Task<IReadOnlyCollection<NearbyPetReportResponse>>
        GetNearbyAsync(
            double latitude,
            double longitude,
            double radiusMeters,
            ReportType? reportType,
            AnimalSpecies? species,
            int limit,
            CancellationToken cancellationToken)
    {
        const string sql = """
            DECLARE @Origin geography =
                geography::Point(@Latitude, @Longitude, 4326);

            SELECT TOP (@Limit)
                pr.Id,
                pr.ReportType,
                pr.Status,
                pr.Title,
                pr.Species,
                pr.Breed,
                pr.PrimaryColor,
                pr.EventDate,
                pr.Location.Lat AS Latitude,
                pr.Location.Long AS Longitude,
                pr.Location.STDistance(@Origin) AS DistanceMeters
            FROM dbo.PetReports AS pr
            WHERE pr.Status = @ActiveStatus
              AND pr.IsArchived = 0
              AND (@ReportType IS NULL OR pr.ReportType = @ReportType)
              AND (@Species IS NULL OR pr.Species = @Species)
              AND pr.Location.STDistance(@Origin) <= @RadiusMeters
            ORDER BY
                DistanceMeters ASC,
                pr.EventDate DESC;
            """;

        var parameters = new
        {
            Latitude = latitude,
            Longitude = longitude,
            RadiusMeters = radiusMeters,
            ReportType = reportType.HasValue
                ? (int?)reportType.Value
                : null,
            Species = species.HasValue
                ? (int?)species.Value
                : null,
            ActiveStatus = (int)ReportStatus.Active,
            Limit = limit
        };

        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<NearbyPetReportRow>(command);

        return rows
            .Select(MapResponse)
            .ToArray();
    }

    private static NearbyPetReportResponse MapResponse(
        NearbyPetReportRow row)
    {
        return new NearbyPetReportResponse
        {
            Id = row.Id,
            ReportType = row.ReportType.ToString(),
            Status = row.Status.ToString(),
            Title = row.Title,
            Species = row.Species.ToString(),
            Breed = row.Breed,
            PrimaryColor = row.PrimaryColor.ToString(),
            EventDate = row.EventDate,
            Latitude = row.Latitude,
            Longitude = row.Longitude,
            DistanceMeters = row.DistanceMeters
        };
    }

    private sealed class NearbyPetReportRow
    {
        public Guid Id { get; init; }
        public ReportType ReportType { get; init; }
        public ReportStatus Status { get; init; }
        public string Title { get; init; } = string.Empty;
        public AnimalSpecies Species { get; init; }
        public string? Breed { get; init; }
        public AnimalColor PrimaryColor { get; init; }
        public DateTimeOffset EventDate { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public double DistanceMeters { get; init; }
    }
}