using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Finance.Repositories;

public sealed class RefundRepository : Repository<Refund>, IRefundRepository
{
    public RefundRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Refund?> GetByPaymentIdAndAmountAsync(
        Guid paymentId,
        Money amount,
        CancellationToken ct = default)
    {
        return await _dbContext.Refunds
            .FirstOrDefaultAsync(x =>
                x.PaymentId == paymentId &&
                x.Amount.Amount == amount.Amount &&
                x.Amount.Currency == amount.Currency,
                ct);
    }

    /// <summary>
    /// Returns non-terminal refunds for a payment — those that have been
    /// requested or are currently being processed. Used to compute the
    /// outstanding refundable total when validating a new refund request.
    /// </summary>
    public async Task<IReadOnlyList<Refund>> GetOutstandingByPaymentIdAsync(
        Guid paymentId,
        CancellationToken ct = default)
    {
        return await _dbContext.Refunds
            .Where(x =>
                x.PaymentId == paymentId &&
                (x.Status == RefundStatus.Requested || x.Status == RefundStatus.Processing))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Returns Requested refunds older than the cutoff. Used by
    /// ReconcileRefundsCommandHandler to recover refunds whose
    /// ProcessRefundJob was never enqueued or was lost.
    /// </summary>
    public async Task<IReadOnlyList<Refund>> GetRequestedOlderThanAsync(
        DateTime cutoffUtc,
        int maxResults,
        CancellationToken ct = default)
    {
        return await _dbContext.Refunds
            .Where(x =>
                x.Status == RefundStatus.Requested &&
                x.RequestedAtUtc < cutoffUtc)
            .OrderBy(x => x.RequestedAtUtc)
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Refund>> GetProcessingOlderThanAsync(
        DateTime cutoffUtc,
        int maxResults,
        CancellationToken ct = default)
    {
        return await _dbContext.Refunds
            .Where(x =>
                x.Status == RefundStatus.Processing &&
                x.RequestedAtUtc < cutoffUtc)
            .OrderBy(x => x.RequestedAtUtc)
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public async Task<bool> TryMarkProcessingAsync(
        Guid refundId,
        CancellationToken ct = default)
    {
        var affected = await _dbContext.Refunds
            .Where(x => x.Id == refundId && x.Status == RefundStatus.Requested)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, RefundStatus.Processing),
                ct);

        return affected > 0;
    }
}