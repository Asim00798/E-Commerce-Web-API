#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.SellerProfile
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
#endif