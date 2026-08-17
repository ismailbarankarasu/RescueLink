namespace RescueLink.API.Contracts.Authentication;

public sealed record RefreshTokenRequest(
    string RefreshToken);