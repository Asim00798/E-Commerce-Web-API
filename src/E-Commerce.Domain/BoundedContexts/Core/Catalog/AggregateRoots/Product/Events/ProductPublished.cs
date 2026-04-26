using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Events
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
