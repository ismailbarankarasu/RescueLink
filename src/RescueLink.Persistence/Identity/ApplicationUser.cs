using Microsoft.AspNetCore.Identity;

namespace RescueLink.Persistence.Identity;

public sealed class ApplicationUser
    : IdentityUser<Guid>
{
    public string FirstName { get; set; } =
        string.Empty;

    public string LastName { get; set; } =
        string.Empty;

    public string? CountryCode { get; set; }

    public string? City { get; set; }

    public string PreferredLanguage { get; set; } =
        "en";

    public string TimeZoneId { get; set; } =
        "UTC";

    public DateTimeOffset CreatedAt { get; set; } =
        DateTimeOffset.UtcNow;
}