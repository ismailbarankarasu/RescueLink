using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.Create;
using RescueLink.Application.Features.PetReports.GetById;
using RescueLink.Application.Features.PetReports.Nearby;
using RescueLink.Domain.Enums;

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

        return CreatedAtRoute(
            routeName: "GetPetReportById",
            routeValues: new
            {
                id = result.Value
            },
            value: new
            {
                PetReportId = result.Value
            });
    }

    [HttpGet("{id:guid}", Name = "GetPetReportById")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var query = new GetPetReportByIdQuery(id);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new
            {
                result.Error.Code,
                result.Error.Message
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("nearby")]
    [AllowAnonymous]
    [ProducesResponseType(
    typeof(IReadOnlyCollection<NearbyPetReportResponse>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<
    IReadOnlyCollection<NearbyPetReportResponse>>> GetNearby(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusMeters = 5_000,
        [FromQuery] ReportType? reportType = null,
        [FromQuery] AnimalSpecies? species = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNearbyPetReportsQuery(
            Latitude: latitude,
            Longitude: longitude,
            RadiusMeters: radiusMeters,
            ReportType: reportType,
            Species: species,
            Limit: limit);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result.Value);
    }
}