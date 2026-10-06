namespace E_Commerce.Infrastructure.Communication.Notifications.Options;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";

    public string AccountSid { get; init; } = string.Empty;
    public string AuthToken { get; init; } = string.Empty;
    public string FromNumber { get; init; } = string.Empty;
}