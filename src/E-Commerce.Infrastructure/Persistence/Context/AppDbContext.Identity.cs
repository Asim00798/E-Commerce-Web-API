
using E_Commerce.Infrastructure.Identity.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        // Identity
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
    }
}
