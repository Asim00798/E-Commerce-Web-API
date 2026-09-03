
namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Enforces the minimum data required for any email notification.
/// </summary>
public interface IEmailNotificationModel : INotificationModel
{
    string RecipientEmail { get; }
    string Subject { get; }
}