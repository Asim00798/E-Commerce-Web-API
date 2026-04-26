#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Entities
{
    public class CampaignRule : BaseEntity
    {
        public string Title { get; private set; }
        public string RuleDefinition { get; private set; }

        public CampaignRule(string title, string ruleDefinition)
        {
            Title = title;
            RuleDefinition = ruleDefinition;
        }
    }
}

#endif