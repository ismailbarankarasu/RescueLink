using Microsoft.AspNetCore.Hosting;
using RescueLink.Application.Abstractions.Storage;

namespace RescueLink.Infrastructure.Storage;

internal sealed class LocalFileStorageService(
    IWebHostEnvironment environment)
    : IFileStorageService
{
    private const long MaximumFileSize = 5 * 1024 * 1024;

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

        var header = new byte[12];

        var headerLength = await ReadHeaderAsync(
            file.Content,
            header,
            cancellationToken);

        var extension = DetectImageExtension(
            header.AsSpan(0, headerLength));

        if (extension is null)
        {
            throw new ArgumentException(
                "The uploaded file is not a valid JPEG, PNG or WebP image.",
                nameof(file));
        }

        var storageKey = Path.Combine(
                "uploads",
                "pet-reports",
                $"{Guid.NewGuid():N}{extension}")
            .Replace('\\', '/');

        var webRootPath = GetWebRootPath();

        var absolutePath = Path.Combine(
            webRootPath,
            storageKey.Replace(
                '/',
                Path.DirectorySeparatorChar));

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

            await destination.WriteAsync(
                header.AsMemory(0, headerLength),
                cancellationToken);
 
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

        var webRootPath = GetWebRootPath();
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

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            return environment.WebRootPath;
        }

        return Path.Combine(
            environment.ContentRootPath,
            "wwwroot");
    }

    private static async Task<int> ReadHeaderAsync(
        Stream content,
        byte[] header,
        CancellationToken cancellationToken)
    {
        var totalBytesRead = 0;

        while (totalBytesRead < header.Length)
        {
            var bytesRead = await content.ReadAsync(
                header.AsMemory(
                    totalBytesRead,
                    header.Length - totalBytesRead),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    private static string? DetectImageExtension(
        ReadOnlySpan<byte> header)
    {
        // JPEG: FF D8 FF
        if (header.Length >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
        {
            return ".jpg";
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header.Length >= 8 &&
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A)
        {
            return ".png";
        }

        // WebP: RIFF....WEBP
        if (header.Length >= 12 &&
            header[0] == 0x52 &&
            header[1] == 0x49 &&
            header[2] == 0x46 &&
            header[3] == 0x46 &&
            header[8] == 0x57 &&
            header[9] == 0x45 &&
            header[10] == 0x42 &&
            header[11] == 0x50)
        {
            return ".webp";
        }

        return null;
    }
}