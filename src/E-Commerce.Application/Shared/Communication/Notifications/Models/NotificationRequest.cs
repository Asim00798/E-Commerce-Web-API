using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Generic wrapper for a durable notification. Binds a user and a typed model.
/// </summary>
public sealed class NotificationRequest<T> where T : INotificationModel
{
    public Guid UserId { get; init; }
    public T Model { get; init; } = default!;
}