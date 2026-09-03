using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string VariantName { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money LineTotal { get; private set; } = null!;

    private OrderItem()
    {
        // EF Core
    }

    public OrderItem(
        Guid productId,
        Guid productVariantId,
        string sku,
        string productName,
        string variantName,
        Money unitPrice,
        int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));
        if (productVariantId == Guid.Empty)
            throw new ArgumentException("Product variant ID is required.", nameof(productVariantId));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));
        if (string.IsNullOrWhiteSpace(variantName))
            throw new ArgumentException("Variant name is required.", nameof(variantName));
        if (unitPrice is null || unitPrice.Amount <= 0)
            throw new ArgumentException("Unit price must be positive.", nameof(unitPrice));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        ProductId = productId;
        ProductVariantId = productVariantId;
        Sku = sku;
        ProductName = productName;
        VariantName = variantName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        LineTotal = unitPrice.WithAmount(unitPrice.Amount * quantity);
    }

    internal void SetOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}