using FluentValidation;

namespace RescueLink.Application.Features
    .PetReportMatches.GetContact;

public sealed class GetMatchContactQueryValidator
    : AbstractValidator<GetMatchContactQuery>
{
    public GetMatchContactQueryValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty();
    }
}