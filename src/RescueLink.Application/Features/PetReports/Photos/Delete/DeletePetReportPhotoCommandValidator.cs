using FluentValidation;

namespace RescueLink.Application.Features.PetReports.Photos.Delete;

public sealed class DeletePetReportPhotoCommandValidator
    : AbstractValidator<DeletePetReportPhotoCommand>
{
    public DeletePetReportPhotoCommandValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();

        RuleFor(x => x.PhotoId)
            .NotEmpty();
    }
}