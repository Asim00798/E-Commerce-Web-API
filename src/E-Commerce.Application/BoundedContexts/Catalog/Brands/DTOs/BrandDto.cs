namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

public record BrandDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
