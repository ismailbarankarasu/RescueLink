using FluentValidation;

namespace RescueLink.Application.Features.PetReports.GetList;

public sealed class GetPetReportsQueryValidator
    : AbstractValidator<GetPetReportsQuery>
{
    public GetPetReportsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);

        RuleFor(x => x.ReportType)
            .IsInEnum()
            .When(x => x.ReportType.HasValue);

        RuleFor(x => x.Species)
            .IsInEnum()
            .When(x => x.Species.HasValue);

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}