namespace E_Commerce.Api.DTOs.v1.Catalog.Brands.Responses;

public sealed class BrandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DescriptionText { get; init; }
    public Guid? LogoFileId { get; init; }
}
