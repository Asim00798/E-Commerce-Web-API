using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Entities;

/// <summary>
/// Represents a product saved in a customer's wishlist.
/// Owned by the Wishlist aggregate; no independent lifecycle.
/// </summary>
public sealed class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    private WishlistItem()
    {
        // EF Core
    }

    internal WishlistItem(Guid wishlistId, Guid productId)
    {
        WishlistId = wishlistId;
        ProductId = productId;
        AddedAtUtc = DateTime.UtcNow;
    }

    internal void SetWishlistId(Guid wishlistId)
    {
        WishlistId = wishlistId;
    }
}