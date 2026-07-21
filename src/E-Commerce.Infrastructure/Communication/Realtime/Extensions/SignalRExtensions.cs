using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Infrastructure.Communication.Realtime.Publishers;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Communication.Realtime.Extensions;

/// <summary>
/// Extension methods for registering SignalR‑related services.
/// </summary>
public static class SignalRExtensions
{
    /// <summary>
    /// Registers the real‑time event publisher.
    /// </summary>
    public static IServiceCollection AddSignalRInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IRealtimeEventPublisher, SignalRRealtimeEventPublisher>();
        return services;
    }
}
