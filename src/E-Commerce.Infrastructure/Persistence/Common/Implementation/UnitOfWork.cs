using Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_Commerce.Infrastructure.Persistence.Common.Implementation
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        public UnitOfWork(AppDbContext dbContext, IDomainEventDispatcher domainEventDispatcher)
        {
            _dbContext = dbContext;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Collect domain events from tracked aggregates
            var domainEvents = _dbContext.ChangeTracker
                .Entries<BaseEntity>()
                .SelectMany(entry => entry.Entity.DomainEvents)
                .ToList();

            // 2. Save initial changes (aggregates, any previous outbox messages)
            int result = await _dbContext.SaveChangesAsync(cancellationToken);

            // 3. Dispatch domain events – may add new Outbox messages to change tracker
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            // 4. Clear domain events from aggregates
            foreach (var entry in _dbContext.ChangeTracker.Entries<BaseEntity>())
            {
                entry.Entity.ClearDomainEvents();
            }

            // 5. Save again to persist newly created Outbox messages
            if (domainEvents.Any())
            {
                result += await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
                throw new InvalidOperationException("Transaction has not been started.");

            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_currentTransaction is not null)
                    await RollbackTransactionAsync();

                _disposed = true;
            }
        }
    }
}