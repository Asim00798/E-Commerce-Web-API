
namespace E_Commerce.Application.Shared.Observability.Tracing;
/// <summary>
/// Minimal abstraction over an explicitly created span.
/// </summary>
public interface ITraceSpan : IDisposable
{
    void AddAttribute(string key, string? value);
    void SetStatus(bool isError, string? description = null);
}
