using FluentValidation;

namespace RescueLink.Application.Features.PetReportMatches.GetMine;

public sealed class GetMyPetReportMatchesQueryValidator
    : AbstractValidator<GetMyPetReportMatchesQuery>
{
    public GetMyPetReportMatchesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}