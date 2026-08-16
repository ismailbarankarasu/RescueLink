using FluentValidation;

namespace RescueLink.Application.Features.PetReports.GetMine;

public sealed class GetMyPetReportsQueryValidator
    : AbstractValidator<GetMyPetReportsQuery>
{
    public GetMyPetReportsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);

        RuleFor(x => x.ReportType)
            .IsInEnum()
            .When(x => x.ReportType.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);
    }
}