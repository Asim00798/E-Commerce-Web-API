using Microsoft.AspNetCore.Builder;

namespace E_Commerce.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring structured logging at the infrastructure level.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog (or another provider) with enrichment and sinks.
    /// </summary>
    public static WebApplicationBuilder AddInfrastructureLogging(this WebApplicationBuilder builder)
    {
        // TODO: Configure Serilog with Console, File, Seq sinks
        return builder;
    }
}
