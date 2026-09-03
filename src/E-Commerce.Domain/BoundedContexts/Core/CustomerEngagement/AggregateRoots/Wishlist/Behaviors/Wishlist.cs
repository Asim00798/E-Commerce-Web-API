using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Entities;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors;

/// <summary>
/// Represents a customer's personal wishlist.
/// A customer can have at most one wishlist.
/// </summary>
public sealed partial class Wishlist : BaseEntity, IAggregateRoot
{
    private readonly List<WishlistItem> _items = new();

    public Guid CustomerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<WishlistItem> Items => _items.AsReadOnly();

    private Wishlist()
    {
        // EF Core
    }

    private Wishlist(Guid customerId)
    {
        CustomerId = customerId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new wishlist for a customer.
    /// </summary>
    public static Wishlist Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new WishlistException("Customer ID is required.");

        return new Wishlist(customerId);
    }

    /// <summary>
    /// Adds a product to the wishlist. Idempotent: if product already exists, it's ignored.
    /// </summary>
    public void AddItem(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new WishlistException("Product ID is required.");

        if (_items.Any(x => x.ProductId == productId))
            return; // already in wishlist, idempotent

        var item = new WishlistItem(Id, productId);
        _items.Add(item);
    }

    /// <summary>
    /// Removes a product from the wishlist. If not present, does nothing.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null)
            return; // ignore

        _items.Remove(item);
    }

    /// <summary>
    /// Clears all items from the wishlist.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }
}