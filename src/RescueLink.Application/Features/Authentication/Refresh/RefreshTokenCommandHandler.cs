using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Application.Features.Authentication.Refresh;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<
        RefreshTokenCommand,
        Result<AuthenticationResponse>>
{
    private readonly IIdentityService
        _identityService;

    public RefreshTokenCommandHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        return await _identityService.RefreshAsync(
            request.RefreshToken.Trim(),
            cancellationToken);
    }
}