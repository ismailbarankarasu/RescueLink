using Microsoft.AspNetCore.Identity;

namespace RescueLink.Persistence.Identity
{
    public sealed class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } =
            DateTimeOffset.UtcNow;
    }
}
