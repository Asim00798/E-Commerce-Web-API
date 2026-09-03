namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public sealed class ProductDescriptionDto
{
    public string Name { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public string? Dimensions { get; init; }
    public string? Weight { get; init; }
    public DateTimeOffset? DateOfManufacture { get; init; }
    public DateTimeOffset? DateOfExpiry { get; init; }
    public string? Material { get; init; }
    public string? Color { get; init; }
}