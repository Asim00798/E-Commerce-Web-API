using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Events;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        public void AddVariant(ProductVariant variant)
        {
            EnsureIsDraft();

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
            AddDomainEvent(new ProductPriceAdjusted(Id, variantId, newPrice));
        }

        public void AdjustVariantStock(Guid variantId, int delta)
        {
            var variant = Variants.FirstOrDefault(v => v.Id == variantId)
                ?? throw new BusinessRuleViolationException("Variant not found.");

            if (variant.StockQuantity + delta < 0)
                throw new BusinessRuleViolationException("Stock cannot be negative.");

            variant.StockQuantity += delta;
        }
    }
}
