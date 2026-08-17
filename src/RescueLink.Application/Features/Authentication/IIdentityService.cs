using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<Result<Guid>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<UserContactInfo?> GetUserContactAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

}