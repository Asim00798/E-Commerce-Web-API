namespace E_Commerce.Infrastructure.Communication.Notifications.Entities;

/// <summary>
/// Persistent record of a user‑facing notification.
/// Written atomically with the business transaction that triggered it.
/// </summary>
public class UserNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;         // e.g. "OrderPlaced"
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }                 // optional structured data
    public Guid SourceEventId { get; set; }                  // ties back to domain event ID
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public bool IsRead { get; set; }
}
