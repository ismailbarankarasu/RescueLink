using FluentValidation;

namespace RescueLink.Application.Features.PetReports.Resolve;

public sealed class ResolvePetReportCommandValidator
    : AbstractValidator<ResolvePetReportCommand>
{
    public ResolvePetReportCommandValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();
    }
}