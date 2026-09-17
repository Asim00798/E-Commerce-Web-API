using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for HTTP-layer rate limiting.
///
/// HTTP rate limiting runs before authentication — it partitions by signals
/// available pre-auth (IP address, endpoint path). Per-user or per-tenant
/// quotas are a business-layer concern and belong in the Application layer,
/// not here.
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Fixed-window policy partitioned by client IP address.
    /// Protects against per-IP abuse across all endpoints.
    /// </summary>
    [Required]
    public RateLimitPolicyOptions Ip { get; init; } = new();

    /// <summary>
    /// Global concurrency limiter that applies to all requests.
    /// Protects against thread-pool and downstream service exhaustion
    /// during bursts, independent of per-IP behavior.
    /// </summary>
    [Required]
    public ConcurrencyPolicyOptions Global { get; init; } = new();
}