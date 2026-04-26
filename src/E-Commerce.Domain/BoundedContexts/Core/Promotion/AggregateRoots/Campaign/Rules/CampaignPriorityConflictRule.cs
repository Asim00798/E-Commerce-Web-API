#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Rules
{
    public class CampaignPriorityConflictRule : IBusinessRule
    {
        private readonly CampaignPriority _priority1;
        private readonly CampaignPriority _priority2;

        public CampaignPriorityConflictRule(CampaignPriority priority1, CampaignPriority priority2)
        {
            _priority1 = priority1;
            _priority2 = priority2;
        }

        public bool IsSatisfied() => _priority1.Value != _priority2.Value;

        public string Message => "Multiple campaigns cannot have the same priority level.";
    }
}

#endif