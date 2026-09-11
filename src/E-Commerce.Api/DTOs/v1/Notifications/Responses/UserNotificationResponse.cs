namespace E_Commerce.Api.DTOs.v1.Notifications.Responses;

/// <summary>
/// Lightweight representation of a user notification for the API.
/// </summary>
public sealed class UserNotificationResponse
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
