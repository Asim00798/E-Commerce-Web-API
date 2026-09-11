using E_Commerce.Application.Shared.Observability.Tracing;
using System.Diagnostics;

namespace E_Commerce.Infrastructure.Observability.Tracing.Spans;

/// <summary>
/// Wraps an Activity behind ITraceSpan.
/// </summary>
public sealed class OpenTelemetrySpan : ITraceSpan
{
    private readonly Activity _activity;

    public OpenTelemetrySpan(Activity activity)
    {
        _activity = activity;
    }

    public void AddAttribute(string key, string? value)
    {
        _activity.SetTag(key, value);
    }

    public void SetStatus(bool isError, string? description = null)
    {
        _activity.SetStatus(
            isError ? ActivityStatusCode.Error : ActivityStatusCode.Ok,
            description);
    }

    public void Dispose()
    {
        _activity.Dispose();
    }
}