namespace E_Commerce.Infrastructure.Communication.Notifications.Messages;

public sealed class SmsMessage
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
