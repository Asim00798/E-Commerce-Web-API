namespace E_Commerce.Domain.DomainEvents.Catalog.Product
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
