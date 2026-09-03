using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;

public sealed partial class Brand : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? DescriptionText { get; private set; }
    public BrandLogo Logo { get; private set; } = null!;

    private Brand()
    {
        // EF Core
    }

    private Brand(string name, string descriptionText, BrandLogo logo)
    {
        Name = name;
        DescriptionText = descriptionText;
        Logo = logo;
    }

    public static Brand Create(
        string name,
        string descriptionText,
        BrandLogo logo)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BrandException("Brand name is required.");

        if (string.IsNullOrWhiteSpace(descriptionText))
            throw new BrandException("Description cannot be null or whitespace.");

        if (descriptionText.Length > 500)
            throw new BrandException("Description cannot exceed 500 characters.");

        if (logo is null)
            throw new BrandException("Logo cannot be null.");

        return new Brand(name, descriptionText, logo);
    }
}