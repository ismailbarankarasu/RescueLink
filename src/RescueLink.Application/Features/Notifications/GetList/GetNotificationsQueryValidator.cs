using FluentValidation;

namespace RescueLink.Application.Features.Notifications.GetList;

public sealed class GetNotificationsQueryValidator
    : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}