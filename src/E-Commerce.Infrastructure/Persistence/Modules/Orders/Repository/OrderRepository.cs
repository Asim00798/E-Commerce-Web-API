using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;


namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Repository
{
    public class OrderRepository : Repository<Order>,IOrderRepository
    {
        public OrderRepository(AppDbContext dbContext) : base(dbContext)
        {}

        public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken ct = default)
        {
            return await _dbContext.Set<Order>()
                .Where(o => o.CustomerId == customerId)
                .ToListAsync(ct);
        }

        public async Task<List<Guid>> GetPendingOrderIdsOlderThanAsync(
        DateTime expirationTime,
        CancellationToken ct = default)
        {
            return await _dbContext.Set<Order>()
                .Where(o => o.Status == OrderStatus.PendingPayment && o.PlacedAtUtc < expirationTime)
                .Select(o => o.Id)
                .ToListAsync(ct);
        }

        // ------------------------------------------------------------------
        // Filtered overloads 
        // ------------------------------------------------------------------

        public async Task<IReadOnlyList<Order>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            OrderStatus? status,
            Guid? customerId,
            CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (status is not null)
                query = query.Where(o => o.Status == status);

            if (customerId is not null)
                query = query.Where(o => o.CustomerId == customerId);

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> GetTotalCountAsync(
            OrderStatus? status,
            Guid? customerId,
            CancellationToken ct = default)
        {
            var query = _dbSet.AsQueryable();

            if (status is not null)
                query = query.Where(o => o.Status == status);

            if (customerId is not null)
                query = query.Where(o => o.CustomerId == customerId);

            return await query.CountAsync(ct);
        }
    }
}
