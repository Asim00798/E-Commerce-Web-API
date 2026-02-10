using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.SellerProfile
{
    public sealed class SellerProfileApproved : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerProfileApproved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}