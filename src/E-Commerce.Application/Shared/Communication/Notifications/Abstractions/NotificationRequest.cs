
namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Generic wrapper for a durable notification that binds a user and a typed model.
/// Serves as the standard parameter contract for all notification channels: combines the
/// target user with the channel‑specific payload, enabling preference checks and message
/// composition in a single call.
/// </summary>
public sealed class NotificationRequest<T> where T : INotificationModel
{
    public Guid UserId { get; init; }
    public T Model { get; init; } = default!;
}