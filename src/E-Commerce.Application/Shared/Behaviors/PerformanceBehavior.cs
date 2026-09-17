using System.Diagnostics;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Shared.Behaviors;

/// <summary>
/// Logs a warning when a MediatR request takes longer than a configured threshold.
/// Logs both successful and failed requests, so slow failures are visible in the log.
/// Uses <see cref="Stopwatch.GetTimestamp"/> and <see cref="Stopwatch.GetElapsedTime(long)"/>
/// to measure elapsed time without allocating a <see cref="Stopwatch"/> per request.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Anything slower than this is logged at Warning level.
    // 500 ms is a common default. Adjust per environment if needed
    // (e.g., 200 ms in production, 1000 ms in development).
    private static readonly TimeSpan SlowRequestThreshold = TimeSpan.FromMilliseconds(500);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Low-allocation timing: Stopwatch.GetTimestamp() returns a long
        // captured from the high-resolution performance counter. No allocation,
        // no state machine overhead.
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            var response = await next();
            LogIfSlow(startTimestamp, succeeded: true);
            return response;
        }
        catch
        {
            LogIfSlow(startTimestamp, succeeded: false);
            throw;
        }
    }

    private void LogIfSlow(long startTimestamp, bool succeeded)
    {
        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
        if (elapsed < SlowRequestThreshold)
            return;

        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId?.ToString() ?? "Anonymous";

        logger.LogWarning(
            "E-Commerce Long Running Request: {RequestName} took {ElapsedMilliseconds} ms and {Outcome} for User {UserId}",
            requestName,
            elapsed.TotalMilliseconds,
            succeeded ? "succeeded" : "failed",
            userId);
    }
}