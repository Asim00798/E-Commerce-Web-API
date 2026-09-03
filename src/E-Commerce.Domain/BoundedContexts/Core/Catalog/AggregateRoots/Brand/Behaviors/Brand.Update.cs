using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;

public sealed partial class Brand
{
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new BrandException("Name cannot be null or whitespace.", nameof(newName));

        Name = newName;
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new BrandException("Description cannot be null or whitespace.", nameof(newDescription));

        DescriptionText = newDescription;
    }

    public void UpdateLogo(BrandLogo newLogo)
    {
        if (newLogo is null)
            throw new BrandException("Logo cannot be null.", nameof(newLogo));

        Logo = newLogo;
    }
}