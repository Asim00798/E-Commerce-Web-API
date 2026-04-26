#if false
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Behaviors.MarketplaceCampaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Repositories
{
    public interface IMarketplaceCampaignRepository
    {
        Task<CampaignAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<CampaignAggregate>> GetAllActiveAsync();
        Task AddAsync(CampaignAggregate campaign);
        Task UpdateAsync(CampaignAggregate campaign);
    }
}

#endif