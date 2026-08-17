using FluentValidation;

namespace RescueLink.Application.Features.Authentication.Refresh;

public sealed class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .WithMessage(
                "Refresh token is required.");
    }
}