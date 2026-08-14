using FluentValidation;

namespace RescueLink.Application.Features.PetReports.Photos.SetPrimary;

public sealed class SetPrimaryPetReportPhotoCommandValidator
    : AbstractValidator<SetPrimaryPetReportPhotoCommand>
{
    public SetPrimaryPetReportPhotoCommandValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();

        RuleFor(x => x.PhotoId)
            .NotEmpty();
    }
}