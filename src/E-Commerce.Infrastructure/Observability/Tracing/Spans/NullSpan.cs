using E_Commerce.Application.Shared.Observability.Tracing;

namespace E_Commerce.Infrastructure.Observability.Tracing.Spans;

/// <summary>
/// No-op span used when no Activity listener is present.
/// </summary>
internal sealed class NullSpan : ITraceSpan
{
    public static readonly NullSpan Instance = new();

    private NullSpan() { }

    public void AddAttribute(string key, string? value) { }

    public void SetStatus(bool isError, string? description = null) { }

    public void Dispose() { }
}