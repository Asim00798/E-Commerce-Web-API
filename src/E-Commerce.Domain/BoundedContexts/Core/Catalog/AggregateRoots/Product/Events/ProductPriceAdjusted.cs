using E_Commerce.Domain.SharedKernel.Events.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Product
{
    public sealed class ProductPriceAdjusted : DomainEvent
    {
        public Guid AggregateId { get; init; }
        public Guid VariantId { get; init; }
        public Money NewPrice { get; init; }

        public ProductPriceAdjusted(Guid aggregateId, Guid variantId, Money newPrice)
        {
            AggregateId = aggregateId;
            VariantId = variantId;
            NewPrice = newPrice;
        }
    }
}
