using System.Diagnostics;
using System.Diagnostics.Metrics;
using MediatR;

namespace E_Commerce.Application.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that records application operation metrics:
/// operation count and duration, with outcome dimension (success/failure).
/// </summary>
public sealed class TelemetryBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Counter<long> _operationCounter;
    private readonly Histogram<double> _operationDuration;

    public TelemetryBehavior(Meter meter)
    {
        // Create instruments from the shared application meter.
        // Since the Meter is a singleton, instruments are effectively shared
        // across behavior instances, even if this behavior is transient.
        _operationCounter = meter.CreateCounter<long>(
            "mediatr.operations",
            unit: "operations",
            description: "Number of MediatR operations.");

        _operationDuration = meter.CreateHistogram<double>(
            "mediatr.operations.duration",
            unit: "ms",
            description: "Duration of MediatR operations.");
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var tags = new TagList
            {
                { "operation", requestName },
                { "outcome", "success" }
            };

            _operationCounter.Add(1, tags);
            _operationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

            return response;
        }
        catch
        {
            stopwatch.Stop();

            var tags = new TagList
            {
                { "operation", requestName },
                { "outcome", "failure" }
            };

            _operationCounter.Add(1, tags);
            _operationDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

            throw;
        }
    }
}