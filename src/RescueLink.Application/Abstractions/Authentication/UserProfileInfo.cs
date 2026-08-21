namespace RescueLink.Application
    .Abstractions.Authentication;

public sealed record UserProfileInfo(
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