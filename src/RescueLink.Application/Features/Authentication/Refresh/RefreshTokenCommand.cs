using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Application.Features.Authentication.Refresh;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : ICommand<Result<AuthenticationResponse>>;