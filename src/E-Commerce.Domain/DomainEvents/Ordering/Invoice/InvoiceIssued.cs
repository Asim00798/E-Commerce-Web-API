using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Invoice
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