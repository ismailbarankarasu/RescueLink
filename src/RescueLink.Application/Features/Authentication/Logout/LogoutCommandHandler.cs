using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Authentication.Logout;

public sealed class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService
        _identityService;

    public LogoutCommandHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        return await _identityService.LogoutAsync(
            request.RefreshToken.Trim(),
            cancellationToken);
    }
}