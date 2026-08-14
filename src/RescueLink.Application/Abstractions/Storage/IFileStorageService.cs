namespace RescueLink.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        FileUpload file,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken);
}