namespace E_Commerce.Infrastructure.Communication.Notifications.Entities;

/// <summary>
/// User preferences for notification channels. Loaded by the dispatcher.
/// </summary>
public sealed class NotificationPreferences
{
    public Guid UserId { get; init; }
    public bool AllowEmail { get; init; }
    public bool AllowSms { get; init; }
    public bool AllowPush { get; init; }
}
