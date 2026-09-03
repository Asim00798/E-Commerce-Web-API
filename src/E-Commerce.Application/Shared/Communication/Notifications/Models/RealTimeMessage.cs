namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// DTO for real‑time events sent via SignalR. Carries ordering metadata for the client.
/// </summary>
public sealed class RealTimeMessage
{
    public Guid UserId { get; init; }
    public string Method { get; init; } = string.Empty;
    public object? Payload { get; init; }

    // -----------------------------------------------------------------
    // The following two properties are NOT consumed by any server‑side code.
    // They are optional metadata that the client may use for ordering,
    // deduplication, or debugging.
    // -----------------------------------------------------------------
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}