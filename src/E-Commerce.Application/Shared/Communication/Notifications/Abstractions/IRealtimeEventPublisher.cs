using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Contract for publishing ephemeral real‑time events to connected clients (SignalR).
/// </summary>
public interface IRealtimeEventPublisher
{
    Task PublishAsync(RealTimeMessage message, CancellationToken cancellationToken = default);
}