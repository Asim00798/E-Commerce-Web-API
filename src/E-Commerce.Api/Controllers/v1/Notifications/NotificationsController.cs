using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Notifications.Responses;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Api.Controllers.v1.Notifications;

/// <summary>
/// User-specific notifications for the authenticated caller.
/// Every action requires authentication; all resources are scoped to the
/// current user's identity.
/// </summary>
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : BaseApiController
{
    private readonly IUserNotificationRepository _notificationRepository;

    public NotificationsController(IUserNotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Returns a paged list of notifications for the current user,
    /// most recent first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserNotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var skip = Math.Max(0, (page - 1) * pageSize);

        var notifications = await _notificationRepository
            .GetByUserIdAsync(userId, skip, pageSize, ct);

        var response = notifications
            .Select(n => new UserNotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc
            })
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Returns the number of unread notifications for the current user.
    /// Response body is a bare integer (e.g. <c>5</c>).
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationRepository.GetUnreadCountAsync(userId, ct);

        return Ok(count);
    }

    /// <summary>
    /// Marks a notification as read. Only the notification's owner can
    /// mark it as read; other users receive 404.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, ct);

        if (notification is null || notification.UserId != GetCurrentUserId())
            return ToNotFoundProblem(id);

        await _notificationRepository.MarkAsReadAsync(id, ct);

        return NoContent();
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Resolves the current user's identifier from the authenticated principal.
    /// Throws if the claim is missing or malformed — this prevents silent
    /// queries against an empty GUID.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claimValue is null || !Guid.TryParse(claimValue, out var userId))
            throw new UnauthorizedAccessException(
                "Authenticated principal is missing a valid NameIdentifier claim.");

        return userId;
    }

    private IActionResult ToNotFoundProblem(Guid notificationId) =>
        Problem(
            title: "Resource not found.",
            detail: $"Notification '{notificationId}' was not found.",
            statusCode: StatusCodes.Status404NotFound);
}