namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Marker interface that links a notification model to a template file.
/// Every typed notification model must implement this interface.
/// </summary>
public interface INotificationModel
{
    string TemplateName { get; }
}