namespace E_Commerce.Application.Shared.Caching;

/// <summary>
/// Represents an error from the caching infrastructure (e.g., Redis).
/// Used by CachingBehavior to safely fall back to the database.
/// </summary>
public sealed class CacheException : Exception
{
    public CacheException(string message, Exception innerException)
        : base(message, innerException)
    {}
}