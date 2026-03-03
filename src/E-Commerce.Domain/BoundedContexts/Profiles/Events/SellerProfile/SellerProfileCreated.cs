using System;

namespace E_Commerce.Domain.BoundedContexts.Profiles.Profiles.SellerProfile
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