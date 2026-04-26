namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public record ProductListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
}
