
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        public DbSet<Order> Orders { get; set; }
    }
}
