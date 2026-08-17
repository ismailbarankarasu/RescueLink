using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Common;
using RescueLink.Persistence.Context;

namespace RescueLink.Persistence.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator
    _refreshTokenGenerator;

    private readonly RescueLinkDbContext
        _dbContext;
    public IdentityService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    RescueLinkDbContext dbContext)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _dbContext = dbContext;
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
        var user = await _userManager.FindByEmailAsync(
            email);

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

        var roles = await _userManager.GetRolesAsync(
            user);

        var accessToken =
            _jwtTokenGenerator.GenerateToken(
                user.Id,
                user.Email!,
                roles.ToArray());

        var generatedRefreshToken =
            _refreshTokenGenerator.Generate();

        var refreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash:
                generatedRefreshToken.TokenHash,
            expiresAt:
                generatedRefreshToken.ExpiresAt);

        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response = new AuthenticationResponse(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email!,
            AccessToken: accessToken.Value,
            ExpiresAt: accessToken.ExpiresAt,
            RefreshToken:
                generatedRefreshToken.Token,
            RefreshTokenExpiresAt:
                generatedRefreshToken.ExpiresAt);

        return Result.Success(response);
    }
    public async Task<UserContactInfo?> GetUserContactAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null ||
            string.IsNullOrWhiteSpace(user.Email))
        {
            return null;
        }

        return new UserContactInfo(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email);
    }

    public async Task<Result<AuthenticationResponse>> RefreshAsync(
    string refreshToken,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<AuthenticationResponse>(
                AuthenticationErrors.InvalidRefreshToken);
        }

        var tokenHash =
            _refreshTokenGenerator.ComputeHash(
                refreshToken.Trim());

        var storedRefreshToken =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token => token.TokenHash == tokenHash,
                    cancellationToken);

        if (storedRefreshToken is null ||
            !storedRefreshToken.IsActive)
        {
            return Result.Failure<AuthenticationResponse>(
                AuthenticationErrors.InvalidRefreshToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            storedRefreshToken.UserId.ToString());

        if (user is null ||
            string.IsNullOrWhiteSpace(user.Email))
        {
            return Result.Failure<AuthenticationResponse>(
                AuthenticationErrors.InvalidRefreshToken);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken =
            _jwtTokenGenerator.GenerateToken(
                user.Id,
                user.Email,
                roles.ToArray());

        var generatedRefreshToken =
            _refreshTokenGenerator.Generate();

        var newRefreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash:
                generatedRefreshToken.TokenHash,
            expiresAt:
                generatedRefreshToken.ExpiresAt);

        storedRefreshToken.Revoke(
            generatedRefreshToken.TokenHash);

        await _dbContext.RefreshTokens.AddAsync(
            newRefreshToken,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var response = new AuthenticationResponse(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email,
            AccessToken: accessToken.Value,
            ExpiresAt: accessToken.ExpiresAt,
            RefreshToken:
                generatedRefreshToken.Token,
            RefreshTokenExpiresAt:
                generatedRefreshToken.ExpiresAt);

        return Result.Success(response);
    }
    public async Task<Result> LogoutAsync(
    string refreshToken,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Success();
        }

        var tokenHash =
            _refreshTokenGenerator.ComputeHash(
                refreshToken.Trim());

        var storedRefreshToken =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token => token.TokenHash == tokenHash,
                    cancellationToken);

        if (storedRefreshToken is null ||
            storedRefreshToken.RevokedAt.HasValue)
        {
            return Result.Success();
        }

        storedRefreshToken.Revoke();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}