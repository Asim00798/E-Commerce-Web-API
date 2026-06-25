using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

public record BrandDto
{
    public Guid Id { get; init; }
    public BrandDescription Description { get; init; } = null!;
}
