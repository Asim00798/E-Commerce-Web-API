#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Entities;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Rules
{
    public class CampaignMustHaveValidConditionsRule : IBusinessRule
    {
        private readonly IEnumerable<CampaignCondition> _conditions;

        public CampaignMustHaveValidConditionsRule(IEnumerable<CampaignCondition> conditions)
        {
            _conditions = conditions;
        }

        public bool IsSatisfied() => _conditions.Any();

        public string Message => "Campaign must have at least one valid condition.";
    }
}

#endif