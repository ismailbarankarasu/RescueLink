using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Abstractions.Storage;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Photos.Delete;

public sealed class DeletePetReportPhotoCommandHandler
    : IRequestHandler<DeletePetReportPhotoCommand, Result>
{
    private readonly IPetReportRepository _petReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public DeletePetReportPhotoCommandHandler(
        IPetReportRepository petReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _petReportRepository = petReportRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(
        DeletePetReportPhotoCommand request,
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

        var photo = petReport.Photos.SingleOrDefault(
            photo => photo.Id == request.PhotoId);

        if (photo is null)
        {
            return Result.Failure(
                PetReportErrors.PhotoNotFound(request.PhotoId));
        }

        var storageKey = photo.StorageKey;

        petReport.RemovePhoto(photo.Id);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _fileStorageService.DeleteAsync(
            storageKey,
            cancellationToken);

        return Result.Success();
    }
}