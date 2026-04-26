#if false
using SellerAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Behaviors.Seller;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Repositories
{
    public interface ISellerRepository
    {
        Task<SellerAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<SellerAggregate>> GetAllActiveAsync();
        Task AddAsync(SellerAggregate seller);
        Task UpdateAsync(SellerAggregate seller);
    }
}

#endif