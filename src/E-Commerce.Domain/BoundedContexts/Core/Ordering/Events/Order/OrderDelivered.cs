using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order
{
    public sealed class OrderDelivered : DomainEvent
    {
        public Guid AggregateId { get; }
        public OrderDelivered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
