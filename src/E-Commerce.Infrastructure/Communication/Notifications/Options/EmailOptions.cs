using MailKit.Security;

namespace E_Commerce.Infrastructure.Communication.Notifications.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public SecureSocketOptions SecureSocketOption { get; init; } = SecureSocketOptions.Auto;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string SenderName { get; init; } = "E-Commerce";
    public string SenderEmail { get; init; } = "noreply@example.com";
    public int TimeoutSeconds { get; init; } = 30;
}