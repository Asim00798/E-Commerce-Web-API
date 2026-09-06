using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using PaymentAggregate = E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors.Payment;

namespace E_Commerce.Infrastructure.Persistence.Modules.Finance.Repositories;

public sealed class PaymentRepository : Repository<PaymentAggregate>, IPaymentRepository
{

    public PaymentRepository(AppDbContext dbContext) : base(dbContext)
    {}

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