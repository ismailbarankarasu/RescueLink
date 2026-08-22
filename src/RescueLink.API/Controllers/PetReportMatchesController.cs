using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Features.PetReportMatches.Confirm;
using RescueLink.Application.Features.PetReportMatches.GetContact;
using RescueLink.Application.Features.PetReportMatches.Reject;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/pet-report-matches")]
[Authorize]
public sealed class PetReportMatchesController(ISender sender): ControllerBase
{
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
            new ConfirmPetReportMatchCommand(matchId);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Code switch
        {
            "Authentication.Unauthenticated" =>
                Unauthorized(),

            "PetReportMatch.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReportMatch.NotFound" =>
                NotFound(result.Error),

            "PetReportMatch.NotSuggested" =>
                Conflict(result.Error),

            _ => BadRequest(result.Error)
        };
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
            new RejectPetReportMatchCommand(matchId);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Code switch
        {
            "Authentication.Unauthenticated" =>
                Unauthorized(),

            "PetReportMatch.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReportMatch.NotFound" =>
                NotFound(result.Error),

            "PetReportMatch.NotSuggested" =>
                Conflict(result.Error),

            _ => BadRequest(result.Error)
        };
    }


    [Authorize]
    [HttpGet("{matchId:guid}/contact")]
    public async Task<IActionResult> GetContact(
    Guid matchId,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetMatchContactQuery(matchId),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Code switch
        {
            "Authentication.Unauthenticated" =>
                Unauthorized(result.Error),

            "PetReportMatch.NotFound" =>
                NotFound(result.Error),

            "PetReportMatch.RelatedReportsNotFound" =>
                NotFound(result.Error),

            "PetReportMatch.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReportMatch.ContactNotAvailable" =>
                Conflict(result.Error),

            _ => BadRequest(result.Error)
        };
    }
}