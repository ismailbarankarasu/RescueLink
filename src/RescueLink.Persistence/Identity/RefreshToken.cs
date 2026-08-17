namespace RescueLink.Persistence.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public bool IsActive =>
        RevokedAt is null &&
        ExpiresAt > DateTimeOffset.UtcNow;

    private RefreshToken()
    {
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            tokenHash);

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Refresh token expiration date must be in the future.",
                nameof(expiresAt));
        }

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash.Trim(),
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Revoke(
        string? replacedByTokenHash = null)
    {
        if (RevokedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Refresh token has already been revoked.");
        }

        RevokedAt = DateTimeOffset.UtcNow;

        ReplacedByTokenHash =
            string.IsNullOrWhiteSpace(replacedByTokenHash)
                ? null
                : replacedByTokenHash.Trim();
    }
}