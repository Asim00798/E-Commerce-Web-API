#if false
using CampaignAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Behaviors.Campaign;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Repositories
{
    public interface ICampaignRepository
    {
        Task<CampaignAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<CampaignAggregate>> GetAllActiveAsync();
        Task AddAsync(CampaignAggregate campaign);
        Task UpdateAsync(CampaignAggregate campaign);
    }
}

#endif