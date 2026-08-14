using FluentValidation;

namespace RescueLink.Application.Features.PetReports.Photos.Upload;

public sealed class UploadPetReportPhotoCommandValidator
    : AbstractValidator<UploadPetReportPhotoCommand>
{
    private const long MaximumFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public UploadPetReportPhotoCommandValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotNull()
            .Must(stream => stream.CanRead)
            .WithMessage("The uploaded file cannot be read.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(contentType => AllowedContentTypes.Contains(
                contentType,
                StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                "Only JPEG, PNG and WebP images are allowed.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaximumFileSize)
            .WithMessage(
                "The uploaded file must be between 1 byte and 5 MB.");
    }
}