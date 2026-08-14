namespace RescueLink.Application.Features.Authentication.Common;

public sealed record AccessToken(
    string Value,
    DateTimeOffset ExpiresAt);