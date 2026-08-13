using FluentValidation;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features.PetReports.Create;

public sealed class CreatePetReportCommandValidator
    : AbstractValidator<CreatePetReportCommand>
{
    public CreatePetReportCommandValidator()
    {
        RuleFor(command => command.ReportType)
            .IsInEnum()
            .WithMessage("Report type is invalid.");

        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(150)
            .WithMessage("Title cannot exceed 150 characters.");

        RuleFor(command => command.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage(
                "Description cannot exceed 2000 characters.");

        RuleFor(command => command.Species)
            .IsInEnum()
            .WithMessage("Animal species is invalid.");

        RuleFor(command => command.Gender)
            .IsInEnum()
            .WithMessage("Animal gender is invalid.");

        RuleFor(command => command.PetName)
            .MaximumLength(100)
            .When(command =>
                !string.IsNullOrWhiteSpace(command.PetName));

        RuleFor(command => command.Breed)
            .MaximumLength(100)
            .When(command =>
                !string.IsNullOrWhiteSpace(command.Breed));

        RuleFor(command => command.PrimaryColor)
            .IsInEnum()
            .WithMessage("Primary color is invalid.");

        RuleFor(command => command.SecondaryColor)
            .IsInEnum()
            .When(command => command.SecondaryColor.HasValue)
            .WithMessage("Secondary color is invalid.");

        RuleFor(command => command.SecondaryColor)
            .NotEqual(command => command.PrimaryColor)
            .When(command => command.SecondaryColor.HasValue)
            .WithMessage(
                "Primary and secondary colors cannot be the same.");

        RuleFor(command => command.EventDate)
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow)
            .WithMessage("Event date cannot be in the future.");

        RuleFor(command => command.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage(
                "Latitude must be between -90 and 90.");

        RuleFor(command => command.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage(
                "Longitude must be between -180 and 180.");

        RuleFor(command => command.Latitude)
            .Must(double.IsFinite)
            .WithMessage("Latitude must be a finite number.");

        RuleFor(command => command.Longitude)
            .Must(double.IsFinite)
            .WithMessage("Longitude must be a finite number.");
    }
}