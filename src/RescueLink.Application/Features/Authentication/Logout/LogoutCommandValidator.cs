using FluentValidation;

namespace RescueLink.Application.Features.Authentication.Logout;

public sealed class LogoutCommandValidator
    : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .WithMessage(
                "Refresh token is required.");
    }
}