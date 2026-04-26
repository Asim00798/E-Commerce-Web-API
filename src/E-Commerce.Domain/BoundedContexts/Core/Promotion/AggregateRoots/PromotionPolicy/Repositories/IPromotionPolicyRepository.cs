#if false
using PromotionPolicyAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Behaviors.PromotionPolicy;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Repositories
{
    public interface IPromotionPolicyRepository
    {
        Task<PromotionPolicyAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<PromotionPolicyAggregate>> GetAllActiveAsync();
        Task AddAsync(PromotionPolicyAggregate policy);
        Task UpdateAsync(PromotionPolicyAggregate policy);
    }
}

#endif