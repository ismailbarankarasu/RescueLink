using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.Create;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/pet-reports")]
[Authorize]
public sealed class PetReportsController : ControllerBase
{
    private readonly ISender _sender;

    public PetReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePetReportCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error ==
                PetReportErrors.Unauthenticated)
            {
                return Unauthorized(new
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
                PetReportId = result.Value
            });
    }
}