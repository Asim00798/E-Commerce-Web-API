using System.Diagnostics.Metrics;

namespace E_Commerce.Infrastructure.Observability.Metrics;

/// <summary>
/// Central application Meter for all custom metrics.
/// This meter is used by typed metric classes and pipeline behaviors.
/// </summary>
public static class ECommerceMeter
{
    public static readonly Meter Instance = new(
        "ECommerce.ModularMonolith",
        "1.0.0");
}