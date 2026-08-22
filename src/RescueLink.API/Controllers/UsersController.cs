using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.API.Common;
using RescueLink.API.Contracts.Users;
using RescueLink.Application.Features.Users.GetCurrent;
using RescueLink.Application.Features.Users.UpdateCurrent;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender, IErrorLocalizer errorLocalizer) : base(errorLocalizer)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(GetCurrentUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var query =
            new GetCurrentUserProfileQuery();

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateCurrentUserProfileRequest request, CancellationToken cancellationToken)
    {
        var command =
            new UpdateCurrentUserProfileCommand(
                FirstName: request.FirstName,
                LastName: request.LastName,
                PhoneNumber: request.PhoneNumber,
                CountryCode: request.CountryCode,
                City: request.City,
                PreferredLanguage:
                    request.PreferredLanguage,
                TimeZoneId: request.TimeZoneId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }
}