using MailKit.Security;

namespace E_Commerce.Infrastructure.Communication.Notifications.Options;

/// <summary>
/// Configuration for the SMTP email transport.
/// </summary>
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

    /// <summary>
    /// Validates the options at startup to catch configuration errors early.
    /// Called during DI registration (see Program.cs).
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new ArgumentException("SMTP Host is required.", nameof(Host));
        if (Port <= 0 || Port > 65535)
            throw new ArgumentException("SMTP Port must be between 1 and 65535.", nameof(Port));
        if (string.IsNullOrWhiteSpace(SenderEmail))
            throw new ArgumentException("Sender email is required.", nameof(SenderEmail));
        if (TimeoutSeconds < 1)
            throw new ArgumentException("Timeout must be at least 1 second.", nameof(TimeoutSeconds));
    }
}