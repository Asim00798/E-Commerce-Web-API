using Domain.SharedKernel.Events;
using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_Commerce.Infrastructure.Persistence.Common.Implementation;

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
        // Collect domain events from tracked aggregates
        var domainEvents = _dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        // Save initial changes (aggregates, outbox messages, etc.)
        int result = await _dbContext.SaveChangesAsync(cancellationToken);

        // Dispatch domain events – this may add outbox messages or post‑commit callbacks
        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        // Clear domain events
        foreach (var entry in _dbContext.ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        // Save again to persist any new outbox messages or state changes
        if (domainEvents.Any())
            result += await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("Transaction has not been started.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;

        // Execute post‑commit application events (SignalR, cache invalidation, etc.)
        await _applicationEventDispatcher.InvokeAsync(_serviceProvider);
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