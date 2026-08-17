using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RescueLink.Application.Abstractions.Authentication;

namespace RescueLink.Infrastructure.Authentication;

internal sealed class RefreshTokenGenerator
    : IRefreshTokenGenerator
{
    private const int TokenByteLength = 64;

    private readonly RefreshTokenOptions _options;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenGenerator(
        IOptions<RefreshTokenOptions> options,
        TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        if (_options.ExpirationDays <= 0)
        {
            throw new InvalidOperationException(
                "Refresh token expiration days must be greater than zero.");
        }
    }

    public GeneratedRefreshToken Generate()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(
                TokenByteLength);

        var token = ConvertToBase64Url(
            randomBytes);

        var tokenHash = ComputeHash(token);

        var expiresAt = _timeProvider
            .GetUtcNow()
            .AddDays(_options.ExpirationDays);

        return new GeneratedRefreshToken(
            Token: token,
            TokenHash: tokenHash,
            ExpiresAt: expiresAt);
    }

    public string ComputeHash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            token);

        var tokenBytes =
            Encoding.UTF8.GetBytes(token);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static string ConvertToBase64Url(
        byte[] bytes)
    {
        return Convert
            .ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}