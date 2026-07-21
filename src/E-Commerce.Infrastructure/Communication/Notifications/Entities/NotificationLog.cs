namespace E_Commerce.Infrastructure.Communication.Notifications.Entities;

/// <summary>
/// Tracks every external notification send for monitoring and troubleshooting.
/// </summary>
public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }                     // integration event that triggered the send
    public string Channel { get; set; } = string.Empty;   // "Email", "Sms", "Push"
    public string Provider { get; set; } = string.Empty;  // "Smtp", "Twilio", "Firebase"
    public string? ProviderMessageId { get; set; }         // optional ID returned by provider
    public string Recipient { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}
