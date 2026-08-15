using FluentValidation;

namespace RescueLink.Application.Features.PetReportMatches.Confirm;

public sealed class ConfirmPetReportMatchCommandValidator
    : AbstractValidator<ConfirmPetReportMatchCommand>
{
    public ConfirmPetReportMatchCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty();
    }
}