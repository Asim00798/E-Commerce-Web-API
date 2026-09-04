using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;

public sealed partial class Product
{
    public void Publish()
    {
        if (Status == ProductStatus.Published)
            return;

        if (Status != ProductStatus.Draft)
            throw new ProductException("Only draft products can be published.");

        Status = ProductStatus.Published;
    }

    public void Draft()
    {
        if (Status == ProductStatus.Draft)
            return;

        if (Status != ProductStatus.Published)
            throw new ProductException("Only published products can be moved to draft.");

        Status = ProductStatus.Draft;
    }

    public void Discontinue()
    {
        if (Status == ProductStatus.Discontinued)
            return;

        Status = ProductStatus.Discontinued;
    }

    public void AddImage(Guid fileId, string? altText = null)
    {
        EnsureIsDraft();

        var image = new ProductImage(Id, fileId, altText);
        _images.Add(image);
    }

    public void SetMainImage(Guid imageId)
    {
        EnsureIsDraft();

        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new BusinessRuleViolationException("Image not found.");

        foreach (var img in _images)
            img.IsMain = false;

        image.IsMain = true;
    }

    public void RemoveImage(Guid imageId)
    {
        EnsureIsDraft();
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is not null)
        {
            _images.Remove(image);
        }
    }

    public void AddVariant(
        string name,
        string? sku,
        Money price,
        int stockQuantity)
    {
        EnsureIsDraft();

        var variant = new ProductVariant(Id, name, sku, price, stockQuantity);

        if (_variants.Any(v => v.SKU == variant.SKU))
            throw new BusinessRuleViolationException("Duplicate SKU.");

        _variants.Add(variant);
    }

    public void UpdatePrice(Guid variantId, Money newPrice)
    {
        EnsureIsDraft();

        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new BusinessRuleViolationException("Variant not found.");

        variant.UpdatePrice(newPrice);
    }

    /// <summary>
    /// Increases stock for a variant.
    /// This is allowed regardless of product status because inventory can be replenished anytime.
    /// </summary>
    public void IncreaseStock(Guid variantId, int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleViolationException("Quantity to increase must be positive.");

        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new BusinessRuleViolationException("Variant not found.");

        variant.IncreaseStock(quantity);
    }

    /// <summary>
    /// Decreases stock for a variant.
    /// This is allowed regardless of product status because orders consume stock.
    /// </summary>
    public void DecreaseStock(Guid variantId, int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleViolationException("Quantity to decrease must be positive.");

        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new BusinessRuleViolationException("Variant not found.");

        variant.DecreaseStock(quantity);
    }

    public void RemoveVariant(Guid variantId)
    {
        EnsureIsDraft();

        var variant = _variants.FirstOrDefault(x => x.Id == variantId);
        if (variant is not null)
        {
            _variants.Remove(variant);
        }
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ProductException("Tag cannot be empty.");

        if (!_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(tag);
        }
    }

    public void RemoveTag(string tag)
    {
        _tags.RemoveAll(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }

    public void UpdateDescription(ProductDescription newDescription)
    {
        EnsureIsDraft();
        Description = newDescription;
    }
}