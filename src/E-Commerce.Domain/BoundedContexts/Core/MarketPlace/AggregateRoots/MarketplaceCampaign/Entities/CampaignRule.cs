#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Entities
{
    public class CampaignRule : BaseEntity
    {
        public string Description { get; private set; }
        public string RuleType { get; private set; }
        public string Condition { get; private set; }

        public CampaignRule(string description, string ruleType, string condition)
        {
            Description = description;
            RuleType = ruleType;
            Condition = condition;
        }
    }
}

#endif