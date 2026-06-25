using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Common.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task AddAsync(T aggregate, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(aggregate, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            return entity != null;
        }

        public virtual Task UpdateAsync(T aggregate, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(aggregate);
            return Task.CompletedTask;
        }

        public virtual void Remove(T aggregate)
        {
            _dbSet.Remove(aggregate);
        }
    }
}