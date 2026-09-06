using System.Diagnostics;

namespace E_Commerce.Infrastructure.Observability.Tracing;

/// <summary>
/// Central application ActivitySource for tracing.
/// Owned by Infrastructure and used by tracing components.
/// </summary>
public static class ECommerceActivitySource
{
    public static readonly ActivitySource Instance = new(
        "ECommerce.ModularMonolith",
        "1.0.0");
}