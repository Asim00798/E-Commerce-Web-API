using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        // Catalog
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
    }
}
