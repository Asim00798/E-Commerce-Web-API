using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;

public interface IRefundRepository : IRepository<Refund>
{
    Task<Refund?> GetByPaymentIdAndAmountAsync(
        Guid paymentId,
        Money amount,
        CancellationToken ct = default);

    Task<IReadOnlyList<Refund>> GetOutstandingByPaymentIdAsync(
        Guid paymentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Refund>> GetRequestedOlderThanAsync(
        DateTime cutoffUtc,
        int maxResults,
        CancellationToken ct = default);

    Task<IReadOnlyList<Refund>> GetProcessingOlderThanAsync(
        DateTime cutoffUtc,
        int maxResults,
        CancellationToken ct = default);

    Task<bool> TryMarkProcessingAsync(
        Guid refundId,
        CancellationToken ct = default);
}