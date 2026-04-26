using E_Commerce.ReadModel.Abstractions;

namespace E_Commerce.ReadModel.BoundedContexts.Ordering.DbContext;

/// <summary>
/// Read-optimised EF Core DbContext for the Ordering bounded context.
/// All queries run with <c>AsNoTracking</c> by default.
/// </summary>
public sealed class OrderingReadDbContext : Microsoft.EntityFrameworkCore.DbContext, IReadDbContext
{
    public OrderingReadDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<OrderingReadDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
    }

    // TODO: Add DbSet<T> read model sets
}
