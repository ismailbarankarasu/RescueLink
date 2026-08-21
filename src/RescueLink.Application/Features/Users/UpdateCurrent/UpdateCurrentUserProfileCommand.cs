using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application
    .Features.Users.UpdateCurrent;

public sealed record UpdateCurrentUserProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CountryCode,
    string? City,
    string PreferredLanguage,
    string TimeZoneId)
    : ICommand<Result>;