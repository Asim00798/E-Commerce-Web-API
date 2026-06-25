namespace E_Commerce.Infrastructure.Observability.Abstractions;

public interface ITraceContext
{
    string TraceId { get; }
    string? SpanId { get; }
}