using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

public sealed record PushNotification : INotificationModel
{
    public string RecipientId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string TemplateName => "PushNotification";
}