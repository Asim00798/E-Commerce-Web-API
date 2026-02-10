using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.SellerProfile
{
    public sealed class SellerProfileCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerProfileCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}