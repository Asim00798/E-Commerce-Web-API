#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Behaviors;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Services
{
    public class CampaignSelectionService
    {
        public Campaign? SelectBestCampaign(IEnumerable<Campaign> campaigns)
        {
            return campaigns.OrderByDescending(c => c.Priority.Value).FirstOrDefault();
        }
    }
}

#endif