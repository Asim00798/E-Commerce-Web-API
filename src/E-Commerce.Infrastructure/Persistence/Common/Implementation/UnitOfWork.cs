using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_Commerce.Infrastructure.Persistence.Common.Implementation;
/// <summary>
/// How it works:
/// Automatic mode (default): Most handlers call only SaveChangesAsync(). The method starts a transaction, performs the two-phase save/dispatch/save, commits, and then runs post-commit callbacks. If any exception occurs, it rolls back.
///
/// Manual mode: If a caller explicitly calls BeginTransactionAsync() first, SaveChangesAsync() detects an existing transaction and does not commit or run post-commit callbacks. The caller must later call CommitTransactionAsync() to commit and execute the callbacks, or RollbackTransactionAsync() on failure.
///
/// Idempotent: Calling SaveChangesAsync() multiple times within the same unit of work is safe; domain events are cleared after each dispatch.
/// </summary>
public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IPostCommitProcessor _applicationEventDispatcher;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        AppDbContext dbContext,
        IDomainEventDispatcher domainEventDispatcher,
        IPostCommitProcessor applicationEventDispatcher,
        IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _applicationEventDispatcher = applicationEventDispatcher;
        _serviceProvider = serviceProvider;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Determine if this call owns the transaction lifecycle.
        bool ownsTransaction = _currentTransaction is null;

        if (ownsTransaction)
        {
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            // Collect domain events from tracked aggregates.
            var domainEvents = _dbContext.ChangeTracker
                .Entries<BaseEntity>()
                .SelectMany(entry => entry.Entity.DomainEvents)
                .ToList();

            // First save: persist business state and any existing outbox messages.
            int result = await _dbContext.SaveChangesAsync(cancellationToken);

            // Dispatch domain events if any. Handlers may add outbox messages or enqueue post-commit callbacks.
            if (domainEvents.Count > 0)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

                // Clear domain events after they have been handled.
                foreach (var entry in _dbContext.ChangeTracker.Entries<BaseEntity>())
                {
                    entry.Entity.ClearDomainEvents();
                }

                // Second save: persist any new outbox messages or state changes produced by handlers.
                result += await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Even if no domain events, clear any (safety).
                foreach (var entry in _dbContext.ChangeTracker.Entries<BaseEntity>())
                {
                    entry.Entity.ClearDomainEvents();
                }
            }

            // If we started the transaction, commit it and then execute post-commit callbacks.
            if (ownsTransaction)
            {
                await _currentTransaction!.CommitAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;

                // Post-commit callbacks (e.g., SignalR hints) run only after successful commit.
                await _applicationEventDispatcher.InvokeAsync(_serviceProvider, cancellationToken);
            }

            return result;
        }
        catch
        {
            // If we own the transaction, roll it back and clean up.
            if (ownsTransaction && _currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
            throw;
        }
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("Transaction has not been started.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;

        // Execute post-commit callbacks (e.g., SignalR hints, cache invalidation).
        await _applicationEventDispatcher.InvokeAsync(_serviceProvider, cancellationToken);
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