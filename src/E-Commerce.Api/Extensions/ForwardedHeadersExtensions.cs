using System.Net;
using E_Commerce.Api.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace E_Commerce.Api.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardedHeadersConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ProxyOptions>()
            .Bind(configuration.GetSection(ProxyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ProxyOptions>, ProxyOptionsValidator>();

        return services;
    }

    /// <summary>
    /// Adds the ForwardedHeaders middleware when <c>Proxy:Enabled</c> is true.
    ///
    /// Must run before any middleware that depends on forwarded request metadata
    /// such as client IP, scheme, or host — for example rate limiting, request
    /// logging, and correlation.
    /// </summary>
    public static IApplicationBuilder UseForwardedHeadersConfiguration(
        this IApplicationBuilder app)
    {
        // Resolve the already-bound, already-validated options. If validation
        // failed at startup, accessing .Value throws and the host does not start.
        var proxyOptions = app.ApplicationServices
            .GetRequiredService<IOptions<ProxyOptions>>()
            .Value;

        if (!proxyOptions.Enabled)
            return app;

        var options = new ForwardedHeadersOptions
        {
            ForwardLimit = proxyOptions.ForwardLimit,
            ForwardedHeaders = ParseHeaders(proxyOptions.ForwardedHeaders)
        };

        // When explicit trust is configured, replace the framework's default
        // loopback-only trust list.
        if (proxyOptions.KnownProxies.Length > 0 || proxyOptions.KnownNetworks.Length > 0)
        {
            options.KnownProxies.Clear();
            options.KnownNetworks.Clear();

            foreach (var proxy in proxyOptions.KnownProxies)
            {
                // Values are validated at startup; parse is guaranteed to succeed.
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }

            foreach (var network in proxyOptions.KnownNetworks)
            {
                // Values are validated at startup; parse is guaranteed to succeed.
                // Aliased IPNetwork resolves to Microsoft.AspNetCore.HttpOverrides.IPNetwork,
                // which is the type ForwardedHeadersOptions.KnownNetworks expects in .NET 8.
                options.KnownNetworks.Add(IPNetwork.Parse(network));
            }
        }

        app.UseForwardedHeaders(options);
        return app;
    }

    private static ForwardedHeaders ParseHeaders(string[] values)
    {
        // Values are validated at startup; parse is guaranteed to succeed.
        var result = ForwardedHeaders.None;

        foreach (var value in values)
        {
            result |= Enum.Parse<ForwardedHeaders>(value, ignoreCase: true);
        }

        return result;
    }
}