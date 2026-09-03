using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Finance.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _dbContext;

    public PaymentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(PaymentAggregate aggregate, CancellationToken ct = default)
    {
        await _dbContext.Payments.AddAsync(aggregate, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Payments.AnyAsync(x => x.Id == id, ct);
    }

    public Task UpdateAsync(PaymentAggregate aggregate, CancellationToken ct = default)
    {
        _dbContext.Payments.Update(aggregate);
        return Task.CompletedTask;
    }

    public void Remove(PaymentAggregate aggregate)
    {
        _dbContext.Payments.Remove(aggregate);
    }

    public async Task<PaymentAggregate?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    public async Task<PaymentAggregate?> GetByProviderIntentionIdAsync(
        string providerIntentionId,
        CancellationToken ct = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.ProviderIntentionId == providerIntentionId, ct);
    }

    public async Task<PaymentAggregate?> GetByProviderTransactionIdAsync(
        string providerTransactionId,
        CancellationToken ct = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.ProviderTransactionId == providerTransactionId, ct);
    }

    public async Task<IReadOnlyList<PaymentAggregate>> GetAwaitingPaymentWithTransactionOlderThanAsync(
        DateTime cutoffUtc,
        int maxResults,
        CancellationToken ct = default)
    {
        return await _dbContext.Payments
            .Where(x =>
                x.Status == PaymentStatus.AwaitingPayment &&
                x.CreatedAt < cutoffUtc &&
                x.ProviderTransactionId != null)
            .OrderBy(x => x.CreatedAt)
            .Take(maxResults)
            .ToListAsync(ct);
    }
}