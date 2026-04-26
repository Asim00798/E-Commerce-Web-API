#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.SellerProfile
{
    public sealed class SellerComplianceRevoked : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerComplianceRevoked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif