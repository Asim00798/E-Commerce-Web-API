namespace E_Commerce.Application.Modules.Notifications.Models;

/// <summary>
/// Data required to register a new push device.
/// </summary>
public sealed class RegisterPushDeviceModel
{
    public Guid UserId { get; init; }
    public string FirebaseInstallationId { get; init; } = string.Empty;
    public string Platform { get; init; } = string.Empty;
}