namespace RescueLink.Application.Abstractions.Authentication;

public sealed record GeneratedRefreshToken(
    string Token,
    string TokenHash,
    DateTimeOffset ExpiresAt);