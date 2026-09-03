using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;

public interface IRatingRepository : IRepository<Rating>
{
    Task<Rating?> GetByCustomerAndProductAsync(
        Guid customerId,
        Guid productId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Rating>> GetByProductIdAsync(
        Guid productId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Rating>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns aggregated rating information for a product without loading all ratings.
    /// The aggregation is performed at the data source.
    /// </summary>
    Task<RatingSummary?> GetProductRatingsSummaryAsync(
        Guid productId,
        CancellationToken ct = default);
}