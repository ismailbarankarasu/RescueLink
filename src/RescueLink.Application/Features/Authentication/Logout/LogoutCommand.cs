using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Authentication.Logout;

public sealed record LogoutCommand(
    string RefreshToken)
    : ICommand<Result>;