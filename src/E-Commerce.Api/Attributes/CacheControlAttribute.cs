namespace E_Commerce.Api.Attributes;

/// <summary>
/// Metadata attribute that defines an HTTP caching policy for an endpoint.
/// It does not contain behavior; the global <c>CacheControlFilter</c> reads it
/// and applies the corresponding Cache-Control header.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true)]
public sealed class CacheControlAttribute : Attribute
{
    /// <summary>
    /// Allows shared caches such as CDNs and proxies to store the response.
    /// </summary>
    public bool Public { get; set; }

    /// <summary>
    /// Restricts the response to a private cache such as the client's browser.
    /// </summary>
    public bool Private { get; set; }

    /// <summary>
    /// Maximum freshness lifetime in seconds.
    /// -1 means the directive is not specified.
    /// </summary>
    public int MaxAge { get; set; } = -1;

    /// <summary>
    /// Maximum freshness lifetime in seconds for shared caches.
    /// -1 means the directive is not specified.
    /// </summary>
    public int SharedMaxAge { get; set; } = -1;

    /// <summary>
    /// Requires caches to revalidate a stale response before reuse.
    /// </summary>
    public bool MustRevalidate { get; set; }

    /// <summary>
    /// Requires caches to validate the response with the origin before reuse.
    /// </summary>
    public bool NoCache { get; set; }

    /// <summary>
    /// Prevents the response from being stored by caches.
    /// </summary>
    public bool NoStore { get; set; }

    /// <summary>
    /// Validates the application's Cache-Control policy rules.
    /// This is executed during application startup rather than per request.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configured policy violates the application's caching rules.
    /// </exception>
    public void Validate()
    {
        if (Public && Private)
        {
            throw new InvalidOperationException(
                "CacheControl cannot be both 'public' and 'private'.");
        }

        if (NoStore &&
            (MaxAge >= 0 ||
             SharedMaxAge >= 0 ||
             Public ||
             Private ||
             NoCache ||
             MustRevalidate))
        {
            throw new InvalidOperationException(
                "CacheControl 'no-store' cannot be combined with other cache directives.");
        }

        if (NoCache &&
            (MaxAge >= 0 || SharedMaxAge >= 0))
        {
            throw new InvalidOperationException(
                "CacheControl 'no-cache' cannot be combined with 'max-age' or 's-maxage'.");
        }

        if (MaxAge < -1)
        {
            throw new InvalidOperationException(
                "CacheControl MaxAge cannot be less than -1.");
        }

        if (SharedMaxAge < -1)
        {
            throw new InvalidOperationException(
                "CacheControl SharedMaxAge cannot be less than -1.");
        }
    }
}