using System;

namespace E_Commerce.Domain.BoundedContexts.Profiles.Profiles.SellerProfile
{
    public sealed class SellerProfileRejected : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerProfileRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}