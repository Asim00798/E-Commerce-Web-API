using E_Commerce.Infrastructure.Security.Authentication.Tokens.Refresh;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        // Authorization (custom)
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Token management (custom)
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}