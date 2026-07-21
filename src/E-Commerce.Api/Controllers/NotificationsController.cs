using Asp.Versioning;
using E_Commerce.Api.DTOs.Shared.Notifications;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]   // requires authentication to access user‑specific notifications
public class NotificationsController : BaseApiController
{
    private readonly IUserNotificationRepository _notificationRepository;

    public NotificationsController(IUserNotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Returns a paged list of notifications for the current user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserNotificationDto>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var skip = (page - 1) * pageSize;
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, skip, pageSize, cancellationToken);
        //var dtos = notifications.Select(MapToDto).ToList();
        return Ok(/*dtos*/);
    }

    /// <summary>
    /// Returns the number of unread notifications for the current user.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(count);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification is null || notification.UserId != GetCurrentUserId())
            return NotFound();

        await _notificationRepository.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : Guid.Empty;
    }

    private static UserNotificationDto MapToDto(UserNotification notification) =>
        new()
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc
        };
}
