namespace E_Commerce.Infrastructure.Communication.Notifications.Messages;

public sealed class PushMessage
{
    public string FirebaseInstallationId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
}