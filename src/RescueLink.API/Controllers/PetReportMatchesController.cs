using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.API.Common;
using RescueLink.Application.Features.PetReportMatches.Confirm;
using RescueLink.Application.Features.PetReportMatches.GetContact;
using RescueLink.Application.Features.PetReportMatches.GetMine;
using RescueLink.Application.Features.PetReportMatches.Reject;
using RescueLink.Domain.Enums;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/pet-report-matches")]
[Authorize]
public sealed class PetReportMatchesController
    : ApiControllerBase
{
    private readonly ISender _sender;

    public PetReportMatchesController(
        ISender sender,
        IErrorLocalizer errorLocalizer)
        : base(errorLocalizer)
    {
        _sender = sender;
    }

    [HttpPatch("{matchId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        var command =
            new ConfirmPetReportMatchCommand(
                matchId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    [HttpPatch("{matchId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        var command =
            new RejectPetReportMatchCommand(
                matchId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    [HttpGet("{matchId:guid}/contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetContact(
        Guid matchId,
        CancellationToken cancellationToken)
    {
        var query =
            new GetMatchContactQuery(
                matchId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("mine")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] MatchStatus? status = null,
    CancellationToken cancellationToken = default)
    {
        var query =
            new GetMyPetReportMatchesQuery(
                Page: page,
                PageSize: pageSize,
                Status: status);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(result.Value);
    }
}