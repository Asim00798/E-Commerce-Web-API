using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.SellerProfile
{
    public sealed class SellerBlocked : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerBlocked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}