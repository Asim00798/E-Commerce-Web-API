using E_Commerce.Application.Shared.Observability.Tracing;
using E_Commerce.Infrastructure.Observability.Tracing.ActivitySource;
using E_Commerce.Infrastructure.Observability.Tracing.Spans;
using System.Diagnostics;

namespace E_Commerce.Infrastructure.Observability.Tracing.Context;

/// <summary>
/// Implementation of ITraceContext using System.Diagnostics.Activity.
/// </summary>
public sealed class OpenTelemetryTraceContext : ITraceContext
{
    public string? CurrentTraceId => Activity.Current?.TraceId.ToString();

    public string? CurrentSpanId => Activity.Current?.SpanId.ToString();

    public ITraceSpan StartSpan(string name, IDictionary<string, string?>? attributes = null)
    {
        var activity = ECommerceActivitySource.Instance.StartActivity(
            name,
            ActivityKind.Internal);

        if (activity is null)
            return NullSpan.Instance;

        if (attributes is not null)
        {
            foreach (var attribute in attributes)
            {
                activity.SetTag(attribute.Key, attribute.Value);
            }
        }

        return new OpenTelemetrySpan(activity);
    }
}