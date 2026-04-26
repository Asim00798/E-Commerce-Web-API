using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Events
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
