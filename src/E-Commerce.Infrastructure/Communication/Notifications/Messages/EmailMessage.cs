namespace E_Commerce.Infrastructure.Communication.Notifications.Messages;

public sealed class EmailMessage
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; } = true;
}
