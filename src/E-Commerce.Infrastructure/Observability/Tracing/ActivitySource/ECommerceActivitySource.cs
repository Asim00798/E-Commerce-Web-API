using System.Diagnostics;
using TracingActivitySource = System.Diagnostics.ActivitySource;

namespace E_Commerce.Infrastructure.Observability.Tracing.ActivitySource;

/// <summary>
/// Central application ActivitySource for tracing.
/// Owned by Infrastructure and used by tracing components.
/// </summary>
public static class ECommerceActivitySource
{
    public static readonly TracingActivitySource Instance = new(
        "ECommerce.ModularMonolith",
        "1.0.0");
}