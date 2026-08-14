using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Features.Authentication;
using RescueLink.Application.Features.Authentication.Login;
using RescueLink.Application.Features.Authentication.Register;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error ==
                AuthenticationErrors.EmailAlreadyInUse)
            {
                return Conflict(new
                {
                    result.Error.Code,
                    result.Error.Message
                });
            }

            return BadRequest(new
            {
                result.Error.Code,
                result.Error.Message
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                UserId = result.Value
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new
            {
                result.Error.Code,
                result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}