using E_Commerce.Domain.SharedKernel.Events.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Product
{
    public sealed class ProductDrafted : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public ProductDrafted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
