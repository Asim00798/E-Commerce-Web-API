using E_Commerce.Application.Shared.Caching;
using E_Commerce.Infrastructure.Caching.Configuration;
using E_Commerce.Infrastructure.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace E_Commerce.Infrastructure.Caching.Extensions;

/// <summary>
/// Registers the Redis-backed implementation of <see cref="ICache"/>.
///
/// Redis is treated as disposable acceleration: the multiplexer is created
/// with <c>AbortOnConnectFail = false</c> so host startup succeeds even when
/// Redis is unreachable. Cache operations then fail with
/// <see cref="CacheException"/>, which the calling code handles by falling
/// through to the database (reads) or swallowing (invalidation).
/// </summary>
public static class RedisCachingExtensions
{
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
                .Bind(configuration.GetSection(RedisOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Configuration),
                    "Redis:Configuration must be provided.")
                .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(CreateMultiplexer);
        services.AddSingleton<ICache, RedisCache>();

        return services;
    }

    // ------------------------------------------------------------------
    // Multiplexer creation
    // ------------------------------------------------------------------

    /// <summary>
    /// Builds the shared <see cref="IConnectionMultiplexer"/> from the bound
    /// <see cref="RedisOptions"/>. Uses <c>AbortOnConnectFail = false</c> so
    /// an unavailable Redis does not fail host startup — the multiplexer
    /// retries in the background, and cache operations fail through
    /// <see cref="CacheException"/> until the connection is established.
    /// </summary>
    private static IConnectionMultiplexer CreateMultiplexer(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

        var configuration = ConfigurationOptions.Parse(options.Configuration);
        configuration.AbortOnConnectFail = false;

        return ConnectionMultiplexer.Connect(configuration);
    }
}