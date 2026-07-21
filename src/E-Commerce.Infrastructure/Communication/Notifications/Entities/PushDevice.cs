
namespace E_Commerce.Infrastructure.Communication.Notifications.Entities
{
    /// <summary>
    /// Represents a registered push notification device for a user.
    /// </summary>
    public class PushDevice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirebaseInstallationId { get; set; } = string.Empty;
        public PushDevicePlatfrom Platform { get; set; } = PushDevicePlatfrom.None; 
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeactivatedAt { get; set; }
    }
}
/// <summary>
/// The platform is provided by the client during device registration
/// (iOS, Android, Web). No server‑side extraction is performed;
/// the backend simply stores whatever the client sends.
/// </summary>
public enum PushDevicePlatfrom
{
    None,
    iOS,
    Android,
    Web
}
