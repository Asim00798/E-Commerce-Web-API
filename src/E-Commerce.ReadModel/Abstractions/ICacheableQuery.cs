namespace E_Commerce.ReadModel.Abstractions;
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
