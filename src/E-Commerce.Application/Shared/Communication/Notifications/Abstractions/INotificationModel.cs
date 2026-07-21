namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Marker interface for typed notification models.
/// Each model must provide a stable template name used by the renderer.
/// </summary>
public interface INotificationModel
{
    string TemplateName { get; }
}