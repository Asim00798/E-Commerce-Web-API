#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.ValueObjects;
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Behaviors.MarketplaceCampaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Factories
{
    public static class CampaignFactory
    {
        public static CampaignAggregate Create(string title, DateTime start, DateTime end, int priority = 1)
        {
            var period = new CampaignPeriod(start, end);
            var campaignPriority = new CampaignPriority(priority);
            return new CampaignAggregate(title, period, campaignPriority);
        }
    }
}

#endif