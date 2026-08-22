using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueLink.API.Common;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.Notifications.GetList;
using RescueLink.Application.Features.Notifications.GetUnreadCount;
using RescueLink.Application.Features.Notifications.MarkAllAsRead;
using RescueLink.Application.Features.Notifications.MarkAsRead;

namespace RescueLink.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController
    : ApiControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(
        ISender sender,
        IErrorLocalizer errorLocalizer)
        : base(errorLocalizer)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<NotificationListItemResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery(
            Page: page,
            PageSize: pageSize,
            UnreadOnly: unreadOnly);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
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

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var query =
            new GetUnreadNotificationCountQuery();

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(new
        {
            Count = result.Value
        });
    }

    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var command =
            new MarkAllNotificationsAsReadCommand();

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(new
        {
            UpdatedCount = result.Value
        });
    }
}