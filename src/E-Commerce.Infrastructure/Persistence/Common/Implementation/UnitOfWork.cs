using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Application.Shared.Exceptions;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_Commerce.Infrastructure.Persistence.Common.Implementation;

/// <summary>
/// How it works:
/// Automatic mode (default): Most handlers call only SaveChangesAsync(). The method starts
/// a transaction, performs the two-phase save/dispatch/save, commits, and then runs
/// post-commit callbacks. If any exception occurs, it rolls back.
///
/// Manual mode: If a caller explicitly calls BeginTransactionAsync() first, SaveChangesAsync()
/// detects an existing transaction and does not commit or run post-commit callbacks. The caller
/// must later call CommitTransactionAsync() to commit and execute the callbacks, or
/// RollbackTransactionAsync() on failure.
///
/// Idempotent: Calling SaveChangesAsync() multiple times within the same unit of work is safe;
/// domain events are cleared after each dispatch.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IPostCommitProcessor _postCommitProcessor;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        AppDbContext dbContext,
        IDomainEventDispatcher domainEventDispatcher,
        IPostCommitProcessor postCommitProcessor,
        IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _postCommitProcessor = postCommitProcessor;
        _serviceProvider = serviceProvider;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("A transaction is already active.");

        _currentTransaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        bool ownsTransaction = _currentTransaction is null;

        if (ownsTransaction)
        {
            _currentTransaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);
        }

        try
        {
            var domainEvents = CollectDomainEvents();

            // First save: persist business state and any pre-existing outbox messages.
            int affected = await SaveChangesInternalAsync(cancellationToken);

            // Dispatch domain events (handlers may add outbox messages or enqueue callbacks).
            if (domainEvents.Count > 0)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            ClearDomainEvents();

            // Second save only if there were domain events (handlers may have added entities).
            if (domainEvents.Count > 0)
            {
                affected += await SaveChangesInternalAsync(cancellationToken);
            }

            if (ownsTransaction)
            {
                await _currentTransaction!.CommitAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;

                await InvokePostCommitCallbacksAsync(cancellationToken);
            }

            return affected;
        }
        catch
        {
            if (ownsTransaction && _currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
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

        await InvokePostCommitCallbacksAsync(cancellationToken);
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
        if (_disposed)
            return;

        if (_currentTransaction is not null)
        {
            await RollbackTransactionAsync();
        }

        _disposed = true;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private List<IDomainEvent> CollectDomainEvents()
    {
        return _dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();
    }

    private void ClearDomainEvents()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }

    /// <summary>
    /// Saves changes and translates EF Core's concurrency exception into the
    /// application-level <see cref="ConcurrencyException"/> so upper layers do not
    /// depend on EF Core.
    /// </summary>
    private async Task<int> SaveChangesInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(
                "The resource was modified by another request. Please refresh and try again.",
                ex);
        }
    }

    /// <summary>
    /// Runs post-commit callbacks. Failures here must not surface as business errors
    /// because the DB transaction has already been committed successfully.
    /// </summary>
    private async Task InvokePostCommitCallbacksAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _postCommitProcessor.InvokeAsync(_serviceProvider, cancellationToken);
        }
        catch
        {
            // Post-commit callbacks are best-effort. The PostCommitProcessor already
            // logs individual failures; this is a final safety net so the caller
            // does not see a business failure after a successful commit.
        }
    }
}