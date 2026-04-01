using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment.Events.Payment
{
    public sealed class PaymentDeclined : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public PaymentDeclined(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}