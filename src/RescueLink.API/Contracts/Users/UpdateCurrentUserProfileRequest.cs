namespace RescueLink.API.Contracts.Users;

public sealed record UpdateCurrentUserProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CountryCode,
    string? City,
    string PreferredLanguage,
    string TimeZoneId);