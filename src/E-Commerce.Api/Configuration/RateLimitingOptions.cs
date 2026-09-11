using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Required]
    public RateLimitPolicyOptions Ip { get; init; } = new();

    [Required]
    public SlidingWindowPolicyOptions User { get; init; } = new();

    [Required]
    public ConcurrencyPolicyOptions Global { get; init; } = new();
}

public sealed class RateLimitPolicyOptions
{
    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 100;

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;

    [Range(0, 1000)]
    public int QueueLimit { get; init; } = 0;
}

public sealed class SlidingWindowPolicyOptions
{
    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 50;

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;

    [Range(1, 10)]
    public int SegmentsPerWindow { get; init; } = 4;

    [Range(0, 1000)]
    public int QueueLimit { get; init; } = 0;
}

public sealed class ConcurrencyPolicyOptions
{
    [Range(1, 10000)]
    public int PermitLimit { get; init; } = 500;

    [Range(0, 1000)]
    public int QueueLimit { get; init; } = 50;
}