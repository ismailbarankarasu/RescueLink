namespace RescueLink.Application.Abstractions.Storage;

public sealed record FileUpload(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);