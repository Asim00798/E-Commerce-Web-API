namespace E_Commerce.Infrastructure.Caching;

public sealed class RedisOptions
{
    public string Configuration { get; set; } = "localhost:6379";
}