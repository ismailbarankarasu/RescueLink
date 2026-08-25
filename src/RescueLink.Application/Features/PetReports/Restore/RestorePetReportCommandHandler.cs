using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features
    .PetReports.Restore;

public sealed class RestorePetReportCommandHandler
    : IRequestHandler<
        RestorePetReportCommand,
        Result>
{
    private readonly IPetReportRepository
        _petReportRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly ICurrentUserService
        _currentUserService;

    public RestorePetReportCommandHandler(
        IPetReportRepository petReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _petReportRepository = petReportRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        RestorePetReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(
                PetReportErrors.Unauthenticated);
        }

        var petReport =
            await _petReportRepository
                .GetByIdIncludingArchivedAsync(
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

        petReport.Restore();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}