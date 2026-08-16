using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Features.PetReports.Update;

public sealed class UpdatePetReportCommandHandler
    : IRequestHandler<UpdatePetReportCommand, Result>
{
    private readonly IPetReportRepository
        _petReportRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly ICurrentUserService
        _currentUserService;

    public UpdatePetReportCommandHandler(
        IPetReportRepository petReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _petReportRepository = petReportRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        UpdatePetReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(
                PetReportErrors.Unauthenticated);
        }

        var petReport =
            await _petReportRepository.GetByIdAsync(
                request.PetReportId,
                cancellationToken);

        if (petReport is null)
        {
            return Result.Failure(
                PetReportErrors.NotFound(
                    request.PetReportId));
        }

        if (petReport.UserId !=
            _currentUserService.UserId.Value)
        {
            return Result.Failure(
                PetReportErrors.Forbidden);
        }

        if (petReport.Status != ReportStatus.Active)
        {
            return Result.Failure(
                PetReportErrors.ReportIsNotActive);
        }

        var location = GeoLocation.Create(
            request.Latitude,
            request.Longitude);

        petReport.UpdateDetails(
            title: request.Title,
            description: request.Description,
            species: request.Species,
            gender: request.Gender,
            petName: request.PetName,
            breed: request.Breed,
            primaryColor: request.PrimaryColor,
            secondaryColor: request.SecondaryColor,
            eventDate: request.EventDate,
            location: location);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}