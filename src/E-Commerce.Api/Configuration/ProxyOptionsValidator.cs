using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace E_Commerce.Api.Configuration;

/// <summary>
/// Validates <see cref="ProxyOptions"/> at startup.
///
/// Data-annotation attributes catch range and required violations. This validator
/// covers the remaining semantic checks — whether strings parse to the types the
/// middleware actually consumes (IP addresses, CIDR networks, ForwardedHeaders
/// enum names). Invalid values cause the host to fail at startup rather than
/// silently degrade at runtime.
///
/// ForwardedHeaders entries are validated by name only. Numeric enum
/// representations (e.g., "1") are rejected, as is the sentinel value "None".
/// A configured proxy feature with no forwarded headers to honor is treated
/// as a misconfiguration.
/// </summary>
public sealed class ProxyOptionsValidator : IValidateOptions<ProxyOptions>
{
    /// <summary>
    /// Valid ForwardedHeaders names, excluding the "None" sentinel.
    /// Comparison is case-insensitive to match the parsing behavior in
    /// <c>ForwardedHeadersExtensions.ParseHeaders</c>.
    /// </summary>
    private static readonly HashSet<string> ValidHeaderNames = new(
        Enum.GetNames<ForwardedHeaders>()
            .Where(n => n != nameof(ForwardedHeaders.None)),
        StringComparer.OrdinalIgnoreCase);

    public ValidateOptionsResult Validate(string? name, ProxyOptions options)
    {
        var failures = new List<string>();

        foreach (var value in options.ForwardedHeaders)
        {
            if (!ValidHeaderNames.Contains(value))
            {
                failures.Add(
                    $"Proxy:ForwardedHeaders contains an invalid value '{value}'. " +
                    $"Valid values are: {string.Join(", ", ValidHeaderNames.OrderBy(n => n))}.");
            }
        }

        foreach (var proxy in options.KnownProxies)
        {
            if (!IPAddress.TryParse(proxy, out _))
            {
                failures.Add(
                    $"Proxy:KnownProxies contains an invalid IP address '{proxy}'.");
            }
        }

        foreach (var network in options.KnownNetworks)
        {
            if (!IPNetwork.TryParse(network, out _))
            {
                failures.Add(
                    $"Proxy:KnownNetworks contains an invalid CIDR network '{network}'.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}