using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;


namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Repository
{
    public class OrderRepository : Repository<Order>,IOrderRepository
    {
        public OrderRepository(AppDbContext dbContext) : base(dbContext)
        {}
        public async Task<List<Order>> GetPendingOrdersOlderThanAsync(TimeSpan timeSpan)
        {
            return await _dbContext.Orders
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < DateTime.UtcNow - timeSpan)
                .ToListAsync();
        }
    }
}
