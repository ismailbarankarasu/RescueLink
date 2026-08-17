using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.Notifications.GetList;
using RescueLink.Application.Features.Notifications.GetUnreadCount;
using RescueLink.Application.Features.Notifications.MarkAsRead;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    ISender sender)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<NotificationListItemResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<
        PagedResult<NotificationListItemResponse>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool unreadOnly = false,
            CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery(
            Page: page,
            PageSize: pageSize,
            UnreadOnly: unreadOnly);

        var result = await sender.Send(
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

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
    Guid id,
    CancellationToken cancellationToken)
    {
        var command =
            new MarkNotificationAsReadCommand(id);

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
                Unauthorized(result.Error),

            "Notification.Forbidden" =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            "Notification.NotFound" =>
                NotFound(result.Error),

            _ => BadRequest(result.Error)
        };
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetUnreadNotificationCountQuery(),
            cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        return Ok(new
        {
            Count = result.Value
        });
    }

}