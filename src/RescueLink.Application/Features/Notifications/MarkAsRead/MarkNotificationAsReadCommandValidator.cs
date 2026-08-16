using FluentValidation;

namespace RescueLink.Application.Features.Notifications.MarkAsRead;

public sealed class MarkNotificationAsReadCommandValidator
    : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty();
    }
}