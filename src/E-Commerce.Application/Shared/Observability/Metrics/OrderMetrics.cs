using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace E_Commerce.Application.Shared.Observability.Metrics;

/// <summary>
/// Typed metric class for order-related business measurements.
/// Instruments are created once using the shared application Meter.
/// </summary>
public sealed class OrderMetrics
{
    private readonly Counter<long> _ordersCreated;
    private readonly Histogram<double> _processingDuration;

    public OrderMetrics(Meter meter)
    {
        _ordersCreated = meter.CreateCounter<long>(
            "orders.created",
            unit: "orders",
            description: "Number of orders created.");

        _processingDuration = meter.CreateHistogram<double>(
            "orders.processing.duration",
            unit: "ms",
            description: "Order processing duration in milliseconds.");
    }

    /// <summary>
    /// Records a completed order creation event.
    /// </summary>
    /// <param name="customerTier">Optional controlled dimension for customer tier.</param>
    public void RecordCreated(string? customerTier = null)
    {
        var tags = new TagList();
        if (!string.IsNullOrWhiteSpace(customerTier))
        {
            tags.Add("customer.tier", customerTier);
        }

        _ordersCreated.Add(1, tags);
    }

    /// <summary>
    /// Records the duration of order processing.
    /// </summary>
    /// <param name="milliseconds">Duration in milliseconds.</param>
    /// <param name="status">Optional controlled dimension for status (e.g., success, failure).</param>
    public void RecordProcessingDuration(double milliseconds, string? status = null)
    {
        var tags = new TagList();
        if (!string.IsNullOrWhiteSpace(status))
        {
            tags.Add("status", status);
        }

        _processingDuration.Record(milliseconds, tags);
    }
}