using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetPendingOrdersOlderThanAsync(TimeSpan timeSpan);
    }
}


