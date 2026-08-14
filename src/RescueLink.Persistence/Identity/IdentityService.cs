using Microsoft.AspNetCore.Identity;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Common;

namespace RescueLink.Persistence.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<Guid>> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return Result.Failure<Guid>(
                AuthenticationErrors.EmailAlreadyInUse);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var identityResult = await _userManager.CreateAsync(
            user,
            password);

        if (!identityResult.Succeeded)
        {
            var description = string.Join(
                " ",
                identityResult.Errors.Select(error =>
                    error.Description));

            return Result.Failure<Guid>(
                AuthenticationErrors.RegistrationFailed(
                    description));
        }

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Result.Failure<AuthenticationResponse>(
                AuthenticationErrors.InvalidCredentials);
        }

        var isPasswordValid =
            await _userManager.CheckPasswordAsync(
                user,
                password);

        if (!isPasswordValid)
        {
            return Result.Failure<AuthenticationResponse>(
                AuthenticationErrors.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Email!,
            roles.ToArray());

        var response = new AuthenticationResponse(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email!,
            AccessToken: accessToken.Value,
            ExpiresAt: accessToken.ExpiresAt);

        return Result.Success(response);
    }
}