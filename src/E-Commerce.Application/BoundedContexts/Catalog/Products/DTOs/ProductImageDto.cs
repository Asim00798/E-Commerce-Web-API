namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public sealed class ProductImageDto
{
    public Guid Id { get; init; }
    public Guid FileId { get; init; }
    public string? AltText { get; init; }
    public bool IsMain { get; init; }
}