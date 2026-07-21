namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Pure application‑layer DTO representing a user‑facing notification.
/// </summary>
public sealed class UserNotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? PayloadJson { get; init; }
    public Guid SourceEventId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? ReadAtUtc { get; init; }
    public bool IsRead { get; init; }
}