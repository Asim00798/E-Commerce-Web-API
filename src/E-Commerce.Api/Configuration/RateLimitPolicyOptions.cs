using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for a fixed-window rate limiting policy.
/// Used to define per-partition request limits over a fixed time window.
/// </summary>
public sealed class RateLimitPolicyOptions
{
    /// <summary>
    /// Maximum number of requests permitted within the window.
    /// </summary>
    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 100;

    /// <summary>
    /// Length of the window in seconds. Once the window elapses, the
    /// permit count resets to <see cref="PermitLimit"/>.
    /// </summary>
    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;

    /// <summary>
    /// Maximum number of requests that may queue when the permit limit
    /// is reached. 0 means requests are rejected immediately without waiting.
    /// </summary>
    [Range(0, 1000)]
    public int QueueLimit { get; init; } = 0;
}