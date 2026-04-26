#if false
using PriceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Behaviors.Price;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Repositories
{
    public interface IPriceRepository
    {
        Task<PriceAggregate?> GetByIdAsync(Guid id);
        Task<PriceAggregate?> GetByProductIdAsync(Guid productId);
        Task AddAsync(PriceAggregate price);
        Task UpdateAsync(PriceAggregate price);
    }
}

#endif