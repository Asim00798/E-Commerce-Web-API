namespace E_Commerce.Application.BoundedContexts.Catalog.Caching;

/// <summary>
/// Cache key factory for the Catalog bounded context.
///
/// Centralizes key construction so cacheable queries and invalidating
/// command handlers reference the same format. Changing a key format
/// is a single-place edit — no risk of read-side and invalidate-side
/// keys drifting apart.
///
/// All keys use the prefix <c>catalog:</c> to namespace them within Redis.
/// </summary>
public static class CatalogCacheKeys
{
    public static string Product(Guid productId) => $"catalog:product:{productId}";
}