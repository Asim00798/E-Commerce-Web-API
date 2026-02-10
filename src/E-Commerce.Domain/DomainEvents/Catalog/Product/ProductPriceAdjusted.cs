namespace E_Commerce.Domain.DomainEvents.Catalog.Product
{
    public sealed class ProductPriceAdjusted : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public ProductPriceAdjusted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
