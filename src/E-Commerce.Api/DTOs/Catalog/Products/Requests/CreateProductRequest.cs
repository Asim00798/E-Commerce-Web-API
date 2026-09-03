namespace E_Commerce.Api.DTOs.Catalog.Products.Requests;

public sealed class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? Dimensions { get; set; }
    public string? Weight { get; set; }
    public DateTimeOffset? DateOfManufacture { get; set; }
    public DateTimeOffset? DateOfExpiry { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public List<string>? Tags { get; set; }
}
