using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Notifications;

namespace NexCommerce.API.Controllers;

[Authorize]
public class NotificationsController(INotificationService notificationService) : ApiControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken ct = default)
    {
        var result = await notificationService.GetUserNotificationsAsync(CurrentUserId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var result = await notificationService.GetUnreadCountAsync(CurrentUserId, ct);
        return Ok(ApiResponse<int>.Ok(result));
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] IEnumerable<Guid> notificationIds, 
        CancellationToken ct = default)
    {
        await notificationService.MarkAsReadAsync(CurrentUserId, notificationIds, ct);
        return Ok(ApiResponse.OkNoData("Notifications marked as read successfully."));
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        await notificationService.MarkAllAsReadAsync(CurrentUserId, ct);
        return Ok(ApiResponse.OkNoData("All notifications marked as read successfully."));
    }
}
