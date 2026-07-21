namespace E_Commerce.Infrastructure.Communication.Notifications.Options;

public sealed class EmailOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = false;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string SenderName { get; init; } = "E-Commerce";
    public string SenderEmail { get; init; } = "noreply@example.com";
}
