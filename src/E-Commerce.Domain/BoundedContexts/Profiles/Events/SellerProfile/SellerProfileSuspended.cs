using System;

namespace E_Commerce.Domain.BoundedContexts.Profiles.Profiles.SellerProfile
{
    public sealed class SellerProfileSuspended : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerProfileSuspended(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}