using MediatR;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Nearby;

public sealed class GetNearbyPetReportsQueryHandler
    : IRequestHandler<
        GetNearbyPetReportsQuery,
        Result<IReadOnlyCollection<NearbyPetReportResponse>>>
{
    private readonly IPetReportReadService _petReportReadService;

    public GetNearbyPetReportsQueryHandler(
        IPetReportReadService petReportReadService)
    {
        _petReportReadService = petReportReadService;
    }

    public async Task<Result<IReadOnlyCollection<NearbyPetReportResponse>>>
        Handle(
            GetNearbyPetReportsQuery request,
            CancellationToken cancellationToken)
    {
        var reports = await _petReportReadService.GetNearbyAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusMeters,
            request.ReportType,
            request.Species,
            request.Limit,
            cancellationToken);

        return Result.Success(reports);
    }
}