namespace E_Commerce.Api.DTOs.v1.Catalog.Brands.Responses;

public sealed class BrandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DescriptionText { get; set; }
    public Guid? LogoFileId { get; set; }
}
