using MediatR;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.GetById;

public sealed class GetPetReportByIdQueryHandler
    : IRequestHandler<
        GetPetReportByIdQuery,
        Result<PetReportDetailResponse>>
{
    private readonly IPetReportRepository _petReportRepository;

    public GetPetReportByIdQueryHandler(
        IPetReportRepository petReportRepository)
    {
        _petReportRepository = petReportRepository;
    }

    public async Task<Result<PetReportDetailResponse>> Handle(
        GetPetReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        var report =
            await _petReportRepository.GetByIdReadOnlyAsync(
                request.Id,
                cancellationToken);

        if (report is null)
        {
            return Result.Failure<PetReportDetailResponse>(
                PetReportErrors.NotFound(request.Id));
        }

        var photos = report.Photos
            .OrderBy(photo => photo.DisplayOrder)
            .Select(photo => new PetReportPhotoResponse(
                Id: photo.Id,
                StorageKey: photo.StorageKey,
                IsPrimary: photo.IsPrimary,
                DisplayOrder: photo.DisplayOrder))
            .ToArray();

        var response = new PetReportDetailResponse(
            Id: report.Id,
            UserId: report.UserId,
            ReportType: report.ReportType,
            Status: report.Status,
            Title: report.Title,
            Description: report.Description,
            Species: report.Species,
            Gender: report.Gender,
            PetName: report.PetName,
            Breed: report.Breed,
            PrimaryColor: report.PrimaryColor,
            SecondaryColor: report.SecondaryColor,
            EventDate: report.EventDate,
            Latitude: report.Location.Latitude,
            Longitude: report.Location.Longitude,
            CreatedAt: report.CreatedAt,
            UpdatedAt: report.UpdatedAt,
            Photos: photos);

        return Result.Success(response);
    }
}