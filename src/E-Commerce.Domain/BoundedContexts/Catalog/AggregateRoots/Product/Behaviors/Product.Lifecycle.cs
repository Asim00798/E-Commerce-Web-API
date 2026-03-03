using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.Catalog.Enums;
using E_Commerce.Domain.Events.Catalog.Product;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors
{
    public partial class Product
    {
        public void Publish()
        {
            EnsureIsDraft();

            if (!_variants.Any())
                throw new BusinessRuleViolationException("Product must have variants.");

            Status = ProductStatus.Published;
            AddDomainEvent(new ProductPublished(Id));
        }

        public void Discontinue()
        {
            if (Status == ProductStatus.Discontinued) return;

            Status = ProductStatus.Discontinued;
            AddDomainEvent(new ProductDiscontinued(Id));
        }
 
        public void AdjustPrice(Guid variantId, Money newPrice)
        {
            if (Status == ProductStatus.Discontinued)
                throw new BusinessRuleViolationException("Cannot adjust price of discontinued product.");

            var variant = _variants.FirstOrDefault(v => v.Id == variantId);
            if (variant is null)
                throw new BusinessRuleViolationException("Variant not found.");

            if (variant.Price == newPrice)
                return; // no change, no event

            UpdatePrice(variant, newPrice);
        }

        // Private helper methods
        private void UpdatePrice(ProductVariant variant, Money newPrice)
        {
            variant.Price = newPrice;
            AddDomainEvent(new ProductPriceAdjusted(Id, variant.Id, newPrice));
        }
    }
}
