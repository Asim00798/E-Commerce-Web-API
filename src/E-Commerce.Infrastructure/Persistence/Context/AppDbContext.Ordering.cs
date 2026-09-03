using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context;

public partial class AppDbContext
{
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
}