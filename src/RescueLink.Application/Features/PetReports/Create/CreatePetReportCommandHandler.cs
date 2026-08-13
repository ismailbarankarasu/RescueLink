using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Entities;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Application.Features.PetReports.Create;

public sealed class CreatePetReportCommandHandler
    : IRequestHandler<CreatePetReportCommand, Result<Guid>>
{
    private readonly IPetReportRepository _petReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreatePetReportCommandHandler(
        IPetReportRepository petReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _petReportRepository = petReportRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        CreatePetReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<Guid>(
                PetReportErrors.Unauthenticated);
        }

        var location = GeoLocation.Create(
            request.Latitude,
            request.Longitude);

        var petReport = PetReport.Create(
            userId: _currentUserService.UserId.Value,
            reportType: request.ReportType,
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

        await _petReportRepository.AddAsync(
            petReport,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success(petReport.Id);
    }
}