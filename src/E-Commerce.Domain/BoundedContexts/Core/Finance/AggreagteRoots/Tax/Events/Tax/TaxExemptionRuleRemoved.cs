#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Tax.Events.Tax
{
    public sealed class TaxExemptionRuleRemoved : DomainEvent
    {
        public Guid AggregateId { get;}

        public TaxExemptionRuleRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif