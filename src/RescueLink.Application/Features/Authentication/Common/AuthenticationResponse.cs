namespace RescueLink.Application.Features.Authentication.Common;

public sealed record AuthenticationResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string AccessToken,
    DateTimeOffset ExpiresAt);