using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    AccessToken GenerateToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles);
}