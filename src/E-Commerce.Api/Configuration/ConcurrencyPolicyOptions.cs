using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for a global concurrency limiter.
/// Limits the number of requests being processed simultaneously,
/// independent of request rate.
/// </summary>
public sealed class ConcurrencyPolicyOptions
{
    /// <summary>
    /// Maximum number of requests that may be in-flight at the same time.
    /// </summary>
    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 500;

    /// <summary>
    /// Maximum number of requests that may queue when all permits are held.
    /// Queued requests are released in the order they arrived.
    /// </summary>
    [Range(0, 1000)]
    public int QueueLimit { get; init; } = 50;
}