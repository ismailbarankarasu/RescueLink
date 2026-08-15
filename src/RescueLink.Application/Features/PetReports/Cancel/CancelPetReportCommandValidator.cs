using FluentValidation;

namespace RescueLink.Application.Features.PetReports.Cancel;

public sealed class CancelPetReportCommandValidator
    : AbstractValidator<CancelPetReportCommand>
{
    public CancelPetReportCommandValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();
    }
}