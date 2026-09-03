using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;

public sealed class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? SKU { get; private set; }
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }

    private ProductVariant()
    {
        // EF Core
    }

    internal ProductVariant(
        Guid productId,
        string name,
        string? sku,
        Money price,
        int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variant name cannot be empty.", nameof(name));

        if (price is null || price.Amount <= 0)
            throw new ArgumentException("Price must be positive.", nameof(price));

        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

        ProductId = productId;
        Name = name;
        SKU = sku;
        Price = price;
        StockQuantity = stockQuantity;
    }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice is null || newPrice.Amount <= 0)
            throw new ArgumentException("Price must be positive.", nameof(newPrice));

        Price = newPrice;
    }

    internal void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleViolationException("Quantity to increase must be positive.");

        StockQuantity += quantity;
    }

    internal void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleViolationException("Quantity to decrease must be positive.");

        if (StockQuantity - quantity < 0)
            throw new BusinessRuleViolationException("Insufficient stock.");

        StockQuantity -= quantity;
    }
}