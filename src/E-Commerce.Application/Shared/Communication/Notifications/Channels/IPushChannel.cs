using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Sends a push notification to all active devices of a user.
/// Implementations are expected to load user preferences, compose the
/// message from a template, and deliver it through a push transport.
/// </summary>
public interface IPushChannel
{
    Task SendAsync(NotificationRequest<PushNotification> request, CancellationToken ct = default);
}