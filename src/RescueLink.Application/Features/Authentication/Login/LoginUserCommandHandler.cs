using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Application.Features.Authentication.Login;

public sealed class LoginUserCommandHandler
    : IRequestHandler<
        LoginUserCommand,
        Result<AuthenticationResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginUserCommandHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(
            email: request.Email.Trim(),
            password: request.Password,
            cancellationToken);
    }
}