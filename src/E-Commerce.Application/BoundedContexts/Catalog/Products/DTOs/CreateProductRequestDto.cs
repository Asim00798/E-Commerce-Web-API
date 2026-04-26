namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public record CreateProductRequestDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? BrandId { get; init; }
}
