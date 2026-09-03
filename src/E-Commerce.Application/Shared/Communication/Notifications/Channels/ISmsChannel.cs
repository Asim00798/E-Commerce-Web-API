using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Channels;

/// <summary>
/// Entry point for SMS notifications. Only models that implement
/// <see cref="ISmsNotificationModel"/> can be sent through this channel.
/// </summary>
public interface ISmsChannel
{
    Task SendAsync<T>(NotificationRequest<T> request, CancellationToken ct = default)
        where T : ISmsNotificationModel;
}