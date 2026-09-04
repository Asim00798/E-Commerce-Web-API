using E_Commerce.Application.Shared.Caching;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace E_Commerce.Infrastructure.Caching;

public static class RedisCachingExtensions
{
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.Configuration);
        });

        services.AddSingleton<ICache, RedisCache>();

        return services;
    }
}