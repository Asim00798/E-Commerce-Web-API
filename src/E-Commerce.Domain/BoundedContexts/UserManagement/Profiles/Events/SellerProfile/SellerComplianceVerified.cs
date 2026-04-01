using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Profiles.Profiles.SellerProfile
{
    public sealed class SellerComplianceVerified : DomainEvent
    {
        public Guid AggregateId { get; }

        public SellerComplianceVerified(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}