using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Application.Features.Authentication.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password)
    : ICommand<Result<AuthenticationResponse>>;