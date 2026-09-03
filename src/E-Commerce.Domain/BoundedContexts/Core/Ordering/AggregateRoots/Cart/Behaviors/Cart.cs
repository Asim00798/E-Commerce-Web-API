using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;

public sealed partial class Cart : BaseEntity, IAggregateRoot
{
    private readonly List<CartItem> _items = new();

    public Guid CustomerId { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
        // EF Core
    }

    private Cart(Guid customerId)
    {
        CustomerId = customerId;
    }

    public static Cart Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new CartException("Customer ID is required.");

        return new Cart(customerId);
    }

    public void AddItem(
        Guid productId,
        Guid productVariantId,
        string sku,
        string productName,
        string variantName,
        Money unitPrice,
        int quantity)
    {
        if (quantity <= 0)
            throw new CartException("Quantity must be positive.");

        var existing = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);

        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
            return;
        }

        var item = new CartItem(
            Id,
            productId,
            productVariantId,
            sku,
            productName,
            variantName,
            unitPrice,
            quantity);

        _items.Add(item);
    }
}