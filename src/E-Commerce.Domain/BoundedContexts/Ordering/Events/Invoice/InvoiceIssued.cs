using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Invoice
{
    public sealed class InvoiceIssued : DomainEvent
    {
        public Guid AggregateId { get; }

        public InvoiceIssued(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}