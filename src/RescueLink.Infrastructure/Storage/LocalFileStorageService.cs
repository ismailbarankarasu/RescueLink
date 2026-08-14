using Microsoft.AspNetCore.Hosting;
using RescueLink.Application.Abstractions.Storage;

namespace RescueLink.Infrastructure.Storage;

internal sealed class LocalFileStorageService(
    IWebHostEnvironment environment)
    : IFileStorageService
{
    private const long MaximumFileSize = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string>
        AllowedContentTypes =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["image/jpeg"] = ".jpg",
                ["image/png"] = ".png",
                ["image/webp"] = ".webp"
            };

    public async Task<string> UploadAsync(
        FileUpload file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(file.Content);

        if (!file.Content.CanRead)
        {
            throw new InvalidOperationException(
                "The uploaded file cannot be read.");
        }

        if (file.Length <= 0)
        {
            throw new ArgumentException(
                "The uploaded file cannot be empty.",
                nameof(file));
        }

        if (file.Length > MaximumFileSize)
        {
            throw new ArgumentException(
                "The uploaded file cannot exceed 5 MB.",
                nameof(file));
        }

        if (!AllowedContentTypes.TryGetValue(
                file.ContentType,
                out var extension))
        {
            throw new ArgumentException(
                "Only JPEG, PNG and WebP images are allowed.",
                nameof(file));
        }

        var storageKey = Path.Combine(
                "uploads",
                "pet-reports",
                $"{Guid.NewGuid():N}{extension}")
            .Replace('\\', '/');

        var webRootPath = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                environment.ContentRootPath,
                "wwwroot");
        }

        var absolutePath = Path.Combine(
            webRootPath,
            storageKey.Replace('/', Path.DirectorySeparatorChar));

        var directoryPath = Path.GetDirectoryName(absolutePath)
            ?? throw new InvalidOperationException(
                "The upload directory could not be determined.");

        Directory.CreateDirectory(directoryPath);

        try
        {
            await using var destination = new FileStream(
                absolutePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await file.Content.CopyToAsync(
                destination,
                cancellationToken);

            return storageKey;
        }
        catch
        {
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }

            throw;
        }
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        cancellationToken.ThrowIfCancellationRequested();

        var webRootPath = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                environment.ContentRootPath,
                "wwwroot");
        }

        var normalizedWebRoot = Path.GetFullPath(webRootPath);

        var absolutePath = Path.GetFullPath(
            Path.Combine(
                normalizedWebRoot,
                storageKey.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));

        var expectedDirectory = Path.GetFullPath(
            Path.Combine(
                normalizedWebRoot,
                "uploads",
                "pet-reports"));

        if (!absolutePath.StartsWith(
                expectedDirectory + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The storage key is invalid.");
        }

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }
}