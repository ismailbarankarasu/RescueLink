using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Authentication.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;

    public RegisterUserCommandHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return await _identityService.RegisterAsync(
            firstName: request.FirstName.Trim(),
            lastName: request.LastName.Trim(),
            email: request.Email.Trim(),
            password: request.Password,
            cancellationToken);
    }
}