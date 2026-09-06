using E_Commerce.Application.Shared.Observability.Tracing;
using MediatR;

namespace E_Commerce.Application.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that creates a span for each request.
/// The span wraps handler execution and records success/failure status.
/// </summary>
public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITraceContext _trace;

    public TracingBehavior(ITraceContext trace)
    {
        _trace = trace;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var span = _trace.StartSpan(
            $"mediatr.{requestName}",
            new Dictionary<string, string?>
            {
                ["request.type"] = requestName
            });

        try
        {
            var response = await next();
            span.SetStatus(false);
            return response;
        }
        catch (Exception ex)
        {
            span.SetStatus(true, ex.Message);
            throw;
        }
    }
}