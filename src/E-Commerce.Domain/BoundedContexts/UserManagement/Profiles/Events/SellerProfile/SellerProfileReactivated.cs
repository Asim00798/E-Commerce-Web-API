#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.SellerProfile
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
#endif