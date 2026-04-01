using E_Commerce.Domain.SharedKernel.Events.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Product
{
    public sealed class ProductPublished : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public ProductPublished(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
