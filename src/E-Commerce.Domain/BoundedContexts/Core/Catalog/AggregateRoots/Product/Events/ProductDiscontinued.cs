using E_Commerce.Domain.SharedKernel.Events.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Product
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
