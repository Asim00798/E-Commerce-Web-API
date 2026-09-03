using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Channels;

/// <summary>
/// Entry point for push notifications. Only models that implement
/// <see cref="IPushNotificationModel"/> can be sent through this channel.
/// </summary>
public interface IPushChannel
{
    Task SendAsync<T>(NotificationRequest<T> request, CancellationToken ct = default)
        where T : IPushNotificationModel;
}