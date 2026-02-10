using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.SellerProfile
{
    public sealed class SellerProfileReactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerProfileReactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}