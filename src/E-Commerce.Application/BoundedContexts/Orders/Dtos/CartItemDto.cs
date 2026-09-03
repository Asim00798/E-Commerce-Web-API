namespace E_Commerce.Application.BoundedContexts.Orders.Dtos;

public sealed class CartItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public Guid ProductVariantId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string VariantName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Quantity { get; init; }
}