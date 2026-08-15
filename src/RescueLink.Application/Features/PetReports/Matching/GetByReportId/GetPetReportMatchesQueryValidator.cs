using FluentValidation;

namespace RescueLink.Application.Features.PetReports
    .Matching.GetByReportId;

public sealed class GetPetReportMatchesQueryValidator
    : AbstractValidator<GetPetReportMatchesQuery>
{
    public GetPetReportMatchesQueryValidator()
    {
        RuleFor(x => x.PetReportId)
            .NotEmpty();
    }
}