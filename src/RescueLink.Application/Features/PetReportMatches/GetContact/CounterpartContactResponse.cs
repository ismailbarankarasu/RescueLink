namespace RescueLink.Application.Features
    .PetReportMatches.GetContact;

public sealed record CounterpartContactResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email);