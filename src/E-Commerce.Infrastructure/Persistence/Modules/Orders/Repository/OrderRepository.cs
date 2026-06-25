using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _dbContext;

        public OrderRepository(AppDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(Order order, CancellationToken cancellationToken)
        {
            await _dbContext.Orders.AddAsync(order, cancellationToken);
            // Note: SaveChanges is called later by UnitOfWork / CommandHandler
        }
    }
}
