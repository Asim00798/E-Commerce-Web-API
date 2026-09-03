using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.CustomerEngagement.Repositories;

public sealed class RatingRepository : Repository<Rating>, IRatingRepository
{
    public RatingRepository(AppDbContext dbContext) : base(dbContext)
    {}
    
    public async Task<Rating?> GetByCustomerAndProductAsync(
        Guid customerId,
        Guid productId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.CustomerId == customerId && r.ProductId == productId, ct);
    }

    public async Task<IReadOnlyList<Rating>> GetByProductIdAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Rating>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves aggregated rating information for a product directly from the data source.
    /// This query performs the aggregation (average, count, and star distribution) in the database
    /// using GROUP BY, avoiding the need to load all individual ratings into application memory.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="RatingSummary"/> containing the average rating, total number of ratings,
    /// and the distribution of star values. Returns <c>null</c> if the product has no ratings.
    /// </returns>
    /// <remarks>
    /// This method is designed for high-performance product pages where loading every rating
    /// entity is unnecessary and potentially expensive. The result is computed as a projection
    /// over the StarRating value, returning only the aggregated summary data.
    /// </remarks>
    public async Task<RatingSummary?> GetProductRatingsSummaryAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        // Database-side aggregation: GROUP BY star value, COUNT and SUM.
        var grouped = await _dbSet
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.StarRating.Value)
            .Select(g => new
            {
                StarValue = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        if (grouped.Count == 0)
            return null;

        var totalCount = grouped.Sum(x => x.Count);
        var totalStars = grouped.Sum(x => x.StarValue * x.Count);
        var average = (double)totalStars / totalCount;

        var distribution = grouped.ToDictionary(x => x.StarValue, x => x.Count);

        return new RatingSummary(
            AverageRating: average,
            TotalCount: totalCount,
            Distribution: distribution);
    }
}