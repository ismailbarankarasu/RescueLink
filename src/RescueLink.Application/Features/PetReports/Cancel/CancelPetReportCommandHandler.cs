using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.Cancel;

public sealed class CancelPetReportCommandHandler
    : IRequestHandler<CancelPetReportCommand, Result>
{
    private readonly IPetReportRepository _petReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelPetReportCommandHandler(
        IPetReportRepository petReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _petReportRepository = petReportRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        CancelPetReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(
                PetReportErrors.Unauthenticated);
        }

        var petReport = await _petReportRepository.GetByIdAsync(
            request.PetReportId,
            cancellationToken);

        if (petReport is null)
        {
            return Result.Failure(
                PetReportErrors.NotFound(request.PetReportId));
        }

        if (petReport.UserId != _currentUserService.UserId.Value)
        {
            return Result.Failure(
                PetReportErrors.Forbidden);
        }

        if (petReport.Status != ReportStatus.Active)
        {
            return Result.Failure(
                PetReportErrors.ReportIsNotActive);
        }

        petReport.Cancel();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}