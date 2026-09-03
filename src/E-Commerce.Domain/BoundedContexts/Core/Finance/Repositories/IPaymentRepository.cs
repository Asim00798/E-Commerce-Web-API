using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Payment?> GetByProviderIntentionIdAsync(string providerIntentionId, CancellationToken ct = default);
    Task<Payment?> GetByProviderTransactionIdAsync(string providerTransactionId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetAwaitingPaymentWithTransactionOlderThanAsync(
    DateTime cutoffUtc,
    int maxResults,
    CancellationToken ct = default);
}