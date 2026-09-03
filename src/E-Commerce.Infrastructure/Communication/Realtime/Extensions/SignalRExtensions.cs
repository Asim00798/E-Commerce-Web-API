using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Infrastructure.Communication.PostCommit.Processing;
using E_Commerce.Infrastructure.Communication.Realtime.Hubs;
using E_Commerce.Infrastructure.Communication.Realtime.Publishers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace E_Commerce.Infrastructure.Communication.Realtime.Extensions;

/// <summary>
/// Extension methods to register all SignalR‑related services and map the hub.
/// </summary>
public static class SignalRExtensions
{
    /// <summary>
    /// Registers SignalR, the post‑commit processor, the real‑time event publisher,
    /// and configures JWT bearer token extraction for WebSocket connections.
    /// <para>
    /// Call this method <b>once</b> during service registration in <c>Program.cs</c>:
    /// <code>
    ///   builder.Services.AddSignalRRealTimeInfrastructure();
    /// </code>
    /// </para>
    /// Ensure AddSignalRInfrastructure is called after
    /// any primary JWT setup (optional but safer)
    /// </summary>
    public static IServiceCollection AddSignalRRealTimeInfrastructure(this IServiceCollection services)
    {
        // Core SignalR services
        services.AddSignalR();

        // Post‑commit processor (executes callbacks after transaction commit)
        services.AddScoped<IPostCommitProcessor, PostCommitProcessor>();

        // Real‑time publisher (SignalR implementation)
        services.AddScoped<IRealtimeEventPublisher, SignalRRealtimeEventPublisher>();

        // Ensure the JWT bearer authentication can extract the access token
        // from the query string when a WebSocket connection is established.
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events ??= new JwtBearerEvents();
            var originalOnMessageReceived = options.Events.OnMessageReceived;
            options.Events.OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                // Fall back to any previously registered handler
                else if (originalOnMessageReceived != null)
                {
                    return originalOnMessageReceived(context);
                }
                return Task.CompletedTask;
            };
        });

        return services;
    }

    /// <summary>
    /// Maps the <see cref="NotificationHub"/> to the HTTP pipeline.
    /// Must be called <b>after</b> <c>app.UseAuthentication()</c> and
    /// <c>app.UseAuthorization()</c>.
    /// <code>
    ///   app.UseAuthentication();
    ///   app.UseAuthorization();
    ///   app.MapSignalRRealTimeHub();
    /// </code>
    /// </summary>
    public static IEndpointRouteBuilder MapSignalRRealTimeHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationHub>("/hubs/notification");
        return endpoints;
    }
}