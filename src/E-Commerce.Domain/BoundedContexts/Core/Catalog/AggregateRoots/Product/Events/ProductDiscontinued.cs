using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Events
{
    public sealed class ProductDiscontinued : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public ProductDiscontinued(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
