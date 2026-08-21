namespace RescueLink.Application.Features.Users.GetCurrent;

public sealed record GetCurrentUserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? CountryCode,
    string? City,
    string PreferredLanguage,
    string TimeZoneId,
    DateTimeOffset CreatedAt);