using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Configuration for proxy-forwarded headers. Bound from the "Proxy" section.
/// Required only when the API runs behind a reverse proxy or load balancer.
/// </summary>
public sealed class ProxyOptions
{
    public const string SectionName = "Proxy";

    /// <summary>
    /// Enables forwarded-header processing. Only set to true when the API
    /// is reachable exclusively through a trusted proxy.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Number of proxies in the forwarding chain to trust. Default 1
    /// (single proxy in front of the API).
    /// </summary>
    [Range(1, 10)]
    public int ForwardLimit { get; init; } = 1;

    /// <summary>
    /// Headers to honor. Valid values (name only, case-insensitive):
    /// XForwardedFor, XForwardedProto, XForwardedHost, XForwardedPrefix, All.
    /// "None" is not permitted.
    /// </summary>
    public string[] ForwardedHeaders { get; init; } =
        ["XForwardedFor", "XForwardedProto"];

    /// <summary>
    /// Trusted proxy IP addresses. Requests from any other address are ignored.
    /// </summary>
    public string[] KnownProxies { get; init; } = [];

    /// <summary>
    /// Trusted proxy networks in CIDR notation (e.g., "10.0.0.0/8").
    /// Use when proxies have dynamic IPs within a known range.
    /// </summary>
    public string[] KnownNetworks { get; init; } = [];
}