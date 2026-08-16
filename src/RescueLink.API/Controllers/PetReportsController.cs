using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.API.Contracts.PetReports;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.PetReports;
using RescueLink.Application.Features.PetReports.Cancel;
using RescueLink.Application.Features.PetReports.Create;
using RescueLink.Application.Features.PetReports.GetById;
using RescueLink.Application.Features.PetReports.GetList;
using RescueLink.Application.Features.PetReports.GetMine;
using RescueLink.Application.Features.PetReports.Matching.GetByReportId;
using RescueLink.Application.Features.PetReports.Nearby;
using RescueLink.Application.Features.PetReports.Photos.Delete;
using RescueLink.Application.Features.PetReports.Photos.SetPrimary;
using RescueLink.Application.Features.PetReports.Photos.Upload;
using RescueLink.Application.Features.PetReports.Resolve;
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

    [HttpPost("{id:guid}/photos")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(
    Guid id,
    [FromForm] UploadPetReportPhotoRequest request,
    CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var contentType = NormalizeContentType(request.File);
        var command = new UploadPetReportPhotoCommand(
            PetReportId: id,
            Content: stream,
            FileName: request.File.FileName,
            ContentType: contentType,
            Length: request.File.Length);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new
            {
                PhotoId = result.Value
            });
        }

        return result.Error.Code switch
        {
            "Authentication.Unauthenticated" =>
                Unauthorized(),

            "PetReport.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReport.NotFound" =>
                NotFound(result.Error),

            "PetReport.InvalidPhotoFile" =>
                BadRequest(result.Error),

            "PetReport.MaximumPhotoCountReached" =>
                BadRequest(result.Error),

            _ => BadRequest(result.Error)
        };
    }
    private static string NormalizeContentType(IFormFile file)
    {
        if (!string.Equals(
                file.ContentType,
                "application/octet-stream",
                StringComparison.OrdinalIgnoreCase))
        {
            return file.ContentType;
        }

        return Path.GetExtension(file.FileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",

            _ => file.ContentType
        };
    }

    [HttpPatch("{reportId:guid}/photos/{photoId:guid}/primary")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryPhoto(
    Guid reportId,
    Guid photoId,
    CancellationToken cancellationToken)
    {
        var command = new SetPrimaryPetReportPhotoCommand(
            PetReportId: reportId,
            PhotoId: photoId);

        var result = await _sender.Send(
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

            "PetReport.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReport.NotFound" =>
                NotFound(result.Error),

            "PetReport.PhotoNotFound" =>
                NotFound(result.Error),

            _ => BadRequest(result.Error)
        };
    }

    [HttpDelete("{reportId:guid}/photos/{photoId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(
    Guid reportId,
    Guid photoId,
    CancellationToken cancellationToken)
    {
        var command = new DeletePetReportPhotoCommand(
            PetReportId: reportId,
            PhotoId: photoId);

        var result = await _sender.Send(
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

            "PetReport.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReport.NotFound" =>
                NotFound(result.Error),

            "PetReport.PhotoNotFound" =>
                NotFound(result.Error),

            _ => BadRequest(result.Error)
        };
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resolve(
    Guid id,
    CancellationToken cancellationToken)
    {
        var command = new ResolvePetReportCommand(id);

        var result = await _sender.Send(
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

            "PetReport.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReport.NotFound" =>
                NotFound(result.Error),

            "PetReport.NotActive" =>
                Conflict(result.Error),

            _ => BadRequest(result.Error)
        };
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
    Guid id,
    CancellationToken cancellationToken)
    {
        var command = new CancelPetReportCommand(id);

        var result = await _sender.Send(
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

            "PetReport.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReport.NotFound" =>
                NotFound(result.Error),

            "PetReport.NotActive" =>
                Conflict(result.Error),

            _ => BadRequest(result.Error)
        };
    }

    [HttpGet("{id:guid}/matches")]
    [Authorize]
    [ProducesResponseType(
    typeof(IReadOnlyCollection<PetReportMatchResponse>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatches(
    Guid id,
    CancellationToken cancellationToken)
    {
        var query = new GetPetReportMatchesQuery(id);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Code switch
        {
            "Authentication.Unauthenticated" =>
                Unauthorized(),

            "PetReport.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "PetReport.NotFound" =>
                NotFound(result.Error),

            _ => BadRequest(result.Error)
        };
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(
    typeof(PagedResult<PetReportListItemResponse>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<
    PagedResult<PetReportListItemResponse>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] ReportType? reportType = null,
        [FromQuery] AnimalSpecies? species = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPetReportsQuery(
            Page: page,
            PageSize: pageSize,
            ReportType: reportType,
            Species: species,
            Search: search);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(
    typeof(PagedResult<MyPetReportListItemResponse>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<
    PagedResult<MyPetReportListItemResponse>>> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] ReportType? reportType = null,
        [FromQuery] ReportStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyPetReportsQuery(
            Page: page,
            PageSize: pageSize,
            ReportType: reportType,
            Status: status);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Authentication.Unauthenticated" =>
                    Unauthorized(result.Error),

                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
    }
}