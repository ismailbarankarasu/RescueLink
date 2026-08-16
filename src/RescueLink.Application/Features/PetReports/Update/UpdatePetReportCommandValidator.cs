using FluentValidation;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.Update;

public sealed class UpdatePetReportCommandValidator
    : AbstractValidator<UpdatePetReportCommand>
{
    public UpdatePetReportCommandValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Species)
            .IsInEnum();

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.PetName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PetName));

        RuleFor(x => x.Breed)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Breed));

        RuleFor(x => x.PrimaryColor)
            .IsInEnum();

        RuleFor(x => x.SecondaryColor)
            .IsInEnum()
            .When(x => x.SecondaryColor.HasValue);

        RuleFor(x => x.SecondaryColor)
            .NotEqual(x => (AnimalColor?)x.PrimaryColor)
            .When(x => x.SecondaryColor.HasValue)
            .WithMessage(
                "Primary and secondary colors cannot be the same.");

        RuleFor(x => x.EventDate)
            .Must(eventDate =>
                eventDate <= DateTimeOffset.UtcNow)
            .WithMessage(
                "Event date cannot be in the future.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);
    }
}