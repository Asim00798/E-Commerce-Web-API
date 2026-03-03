using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Invoice
{
    public sealed class InvoicePaid : DomainEvent
    {
        public Guid AggregateId { get; }

        public InvoicePaid(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}