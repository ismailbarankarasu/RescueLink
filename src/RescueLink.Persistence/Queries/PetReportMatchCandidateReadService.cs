using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Features.PetReports.Matching;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class PetReportMatchCandidateReadService(
    IDbConnectionFactory connectionFactory)
    : IPetReportMatchCandidateReadService
{
    public async Task<IReadOnlyCollection<PetReportMatchCandidate>>
        GetCandidatesAsync(
            Guid sourceReportId,
            Guid sourceUserId,
            ReportType candidateReportType,
            AnimalSpecies species,
            double latitude,
            double longitude,
            double maximumDistanceMeters,
            int limit,
            CancellationToken cancellationToken)
    {
        const string sql = """
            DECLARE @Origin geography =
                geography::Point(@Latitude, @Longitude, 4326);

            SELECT TOP (@Limit)
                report.Id AS PetReportId,
                report.Location.STDistance(@Origin) AS DistanceMeters
            FROM dbo.PetReports AS report
            WHERE report.Id <> @SourceReportId
              AND report.UserId <> @SourceUserId
              AND report.Status = @ActiveStatus
              AND report.ReportType = @CandidateReportType
              AND report.Species = @Species
              AND report.Location.STDistance(@Origin)
                    <= @MaximumDistanceMeters
            ORDER BY
                DistanceMeters ASC,
                report.EventDate DESC;
            """;

        var parameters = new
        {
            SourceReportId = sourceReportId,
            SourceUserId = sourceUserId,
            CandidateReportType = (int)candidateReportType,
            Species = (int)species,
            ActiveStatus = (int)ReportStatus.Active,
            Latitude = latitude,
            Longitude = longitude,
            MaximumDistanceMeters = maximumDistanceMeters,
            Limit = limit
        };

        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var candidates =
            await connection.QueryAsync<PetReportMatchCandidate>(
                command);

        return candidates.ToArray();
    }
}