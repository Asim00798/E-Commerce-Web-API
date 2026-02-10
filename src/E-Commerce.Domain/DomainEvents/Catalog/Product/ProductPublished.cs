namespace E_Commerce.Domain.DomainEvents.Catalog.Product
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
