
namespace E_Commerce.Api.Attributes;

/// <summary>
/// Metadata attribute that defines an HTTP caching policy for an endpoint.
/// It does not contain behavior; the global CacheControlFilter reads it and applies headers.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class CacheControlAttribute : Attribute
{
    public bool Public { get; set; }
    public bool Private { get; set; }
    public int MaxAge { get; set; } = -1;          // -1 = not set
    public int SharedMaxAge { get; set; } = -1;    // -1 = not set
    public bool MustRevalidate { get; set; }
    public bool NoCache { get; set; }
    public bool NoStore { get; set; }

    /// <summary>
    /// Validates the policy after binding. Throws if the combination is invalid.
    /// Called by the filter before generating the header.
    /// </summary>
    public void Validate()
    {
        if (Public && Private)
            throw new InvalidOperationException("CacheControl cannot be both public and private.");

        if (NoStore && (MaxAge >= 0 || SharedMaxAge >= 0 || Public || Private || NoCache || MustRevalidate))
            throw new InvalidOperationException("no-store cannot be combined with other cache directives.");

        if (NoCache && (MaxAge >= 0 || SharedMaxAge >= 0))
            throw new InvalidOperationException("no-cache cannot be combined with max-age or s-maxage.");

        if (NoCache && MustRevalidate)
            throw new InvalidOperationException("no-cache should not be combined with must-revalidate.");

        if (MaxAge < -1)
            throw new InvalidOperationException("MaxAge cannot be less than -1.");

        if (SharedMaxAge < -1)
            throw new InvalidOperationException("SharedMaxAge cannot be less than -1.");
    }
}