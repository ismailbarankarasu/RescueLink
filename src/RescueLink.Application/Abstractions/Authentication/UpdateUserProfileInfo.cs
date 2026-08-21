namespace RescueLink.Application
    .Abstractions.Authentication;

public sealed record UpdateUserProfileInfo(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CountryCode,
    string? City,
    string PreferredLanguage,
    string TimeZoneId);