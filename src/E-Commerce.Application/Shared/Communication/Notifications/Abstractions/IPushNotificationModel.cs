using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Enforces the minimum data required for any push notification.
/// </summary>
public interface IPushNotificationModel : INotificationModel
{
    string RecipientId { get; }
    string Title { get; }
    string Body { get; }
}