namespace RescueLink.Application
    .Abstractions.Authentication;

public sealed record UserContactInfo(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email);