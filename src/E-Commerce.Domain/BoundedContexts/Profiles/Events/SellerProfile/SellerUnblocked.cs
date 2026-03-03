using System;

namespace E_Commerce.Domain.BoundedContexts.Profiles.Profiles.SellerProfile
{
    public sealed class SellerUnblocked : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerUnblocked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}