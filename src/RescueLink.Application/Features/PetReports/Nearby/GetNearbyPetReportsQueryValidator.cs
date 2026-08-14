using FluentValidation;

namespace RescueLink.Application.Features.PetReports.Nearby;

public sealed class GetNearbyPetReportsQueryValidator
    : AbstractValidator<GetNearbyPetReportsQuery>
{
    public GetNearbyPetReportsQueryValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.RadiusMeters)
            .GreaterThan(0)
            .LessThanOrEqualTo(50_000);

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.ReportType)
            .IsInEnum()
            .When(x => x.ReportType.HasValue);

        RuleFor(x => x.Species)
            .IsInEnum()
            .When(x => x.Species.HasValue);
    }
}