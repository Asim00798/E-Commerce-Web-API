using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Enforces the minimum data required for any SMS notification.
/// </summary>
public interface ISmsNotificationModel : INotificationModel
{
    string PhoneNumber { get; }
    string Text { get; }
}