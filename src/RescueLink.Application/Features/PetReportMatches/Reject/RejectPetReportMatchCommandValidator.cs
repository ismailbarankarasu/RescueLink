using FluentValidation;

namespace RescueLink.Application.Features.PetReportMatches.Reject;

public sealed class RejectPetReportMatchCommandValidator
    : AbstractValidator<RejectPetReportMatchCommand>
{
    public RejectPetReportMatchCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty();
    }
}