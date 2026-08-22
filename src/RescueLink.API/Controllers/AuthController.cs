using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RescueLink.API.Common;
using RescueLink.API.Contracts.Authentication;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Login;
using RescueLink.Application.Features.Authentication.Logout;
using RescueLink.Application.Features.Authentication.Refresh;
using RescueLink.Application.Features.Authentication.Register;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IErrorLocalizer _errorLocalizer;

    public AuthController(ISender sender, IErrorLocalizer errorLocalizer)
    {
        _sender = sender;
        _errorLocalizer = errorLocalizer;
    }

    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            var localizedError =
                _errorLocalizer.Localize(
                    result.Error);

            if (result.Error ==
                AuthenticationErrors.EmailAlreadyInUse)
            {
                return Conflict(localizedError);
            }

            return BadRequest(localizedError);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                UserId = result.Value
            });
    }

    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            var localizedError =
                _errorLocalizer.Localize(
                    result.Error);

            return Unauthorized(localizedError);
        }

        return Ok(result.Value);
    }

    [EnableRateLimiting(RateLimitPolicies.Token)]
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RefreshTokenCommand(
                request.RefreshToken),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var localizedError =
            _errorLocalizer.Localize(
                result.Error);

        return result.Error.Code switch
        {
            "Authentication.InvalidRefreshToken" =>
                Unauthorized(localizedError),

            _ => BadRequest(localizedError)
        };
    }

    [EnableRateLimiting(RateLimitPolicies.Token)]
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LogoutCommand(
                request.RefreshToken),
            cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        var localizedError =
            _errorLocalizer.Localize(
                result.Error);

        return BadRequest(localizedError);
    }
}