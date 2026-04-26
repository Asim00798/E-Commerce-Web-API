#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects;
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Behaviors.Campaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Factories
{
    public static class CampaignFactory
    {
        public static CampaignAggregate Create(string name, DateTime start, DateTime end, int priority = 1)
        {
            var campaignName = new CampaignName(name);
            var period = new CampaignPeriod(start, end);
            var campaignPriority = new CampaignPriority(priority);
            return new CampaignAggregate(campaignName, period, campaignPriority);
        }
    }
}

#endif