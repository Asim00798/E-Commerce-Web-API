using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Behaviors;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Context;

public partial class AppDbContext
{
    public DbSet<Rating> Ratings { get; set; } = null!;
    public DbSet<Wishlist> Wishlists { get; set; } = null!;

    // No DbSet<WishlistItem>; items are managed through Wishlist aggregate.
}