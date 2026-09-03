namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

public sealed class BrandDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? DescriptionText { get; init; }

    public Guid LogoFileId { get; init; }
}