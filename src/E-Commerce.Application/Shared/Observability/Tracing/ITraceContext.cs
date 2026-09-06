
namespace E_Commerce.Application.Shared.Observability.Tracing;

/// <summary>
/// Application-facing access to current trace/span context
/// and optional child-span creation.
/// </summary>
public interface ITraceContext
{
    ITraceSpan StartSpan(string name, IDictionary<string, string?>? attributes = null);

    string? CurrentTraceId { get; }
    string? CurrentSpanId { get; }
}