using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Abstractions.Storage;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Photos.Upload;

public sealed class UploadPetReportPhotoCommandHandler
    : IRequestHandler<UploadPetReportPhotoCommand, Result<Guid>>
{
    private readonly IPetReportRepository _petReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UploadPetReportPhotoCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        UploadPetReportPhotoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<Guid>(
                PetReportErrors.Unauthenticated);
        }

        var petReport = await _petReportRepository.GetByIdAsync(
            request.PetReportId,
            cancellationToken);

        if (petReport is null)
        {
            return Result.Failure<Guid>(
                PetReportErrors.NotFound(request.PetReportId));
        }

        if (petReport.UserId != _currentUserService.UserId.Value)
        {
            return Result.Failure<Guid>(
                PetReportErrors.Forbidden);
        }

        if (!petReport.CanAddPhoto)
        {
            return Result.Failure<Guid>(
                PetReportErrors.MaximumPhotoCountReached);
        }

        var file = new FileUpload(
        Content: request.Content,
        FileName: request.FileName,
        ContentType: request.ContentType,
        Length: request.Length);

        string storageKey;

        try
        {
            storageKey = await _fileStorageService.UploadAsync(
                file,
                cancellationToken);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(
                PetReportErrors.InvalidPhotoFile);
        }

        try
        {
            petReport.AddPhoto(storageKey);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            var photoId = petReport.Photos
                .Single(photo => photo.StorageKey == storageKey)
                .Id;

            return Result.Success(photoId);
        }
        catch
        {
            await _fileStorageService.DeleteAsync(
                storageKey,
                CancellationToken.None);

            throw;
        }
    }
}