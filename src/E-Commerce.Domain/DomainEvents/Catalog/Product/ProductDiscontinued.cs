namespace E_Commerce.Domain.DomainEvents.Catalog.Product
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
