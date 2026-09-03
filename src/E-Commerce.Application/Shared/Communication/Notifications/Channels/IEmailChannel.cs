using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Channels;

/// <summary>
/// Entry point for email notifications. Only models that implement
/// <see cref="IEmailNotificationModel"/> can be sent through this channel.
/// </summary>
public interface IEmailChannel
{
    Task SendAsync<T>(NotificationRequest<T> request, CancellationToken ct = default)
        where T : IEmailNotificationModel;
}