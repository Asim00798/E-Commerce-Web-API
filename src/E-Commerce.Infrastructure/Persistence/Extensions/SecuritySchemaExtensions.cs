
using E_Commerce.Infrastructure.Identity.Entities;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Persistence.Extensions
{
    public static class SecuritySchemaConfig
    {
        public static void RenameSecurityTables(this ModelBuilder builder)
        {
            builder.Entity<User>().ToTable("Users", "Security");
            builder.Entity<Role>().ToTable("Roles", "Security");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "Security");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "Security");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "Security");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "Security");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "Security");

            builder.Entity<Permission>().ToTable("Permissions", "Security");
            builder.Entity<RolePermission>().ToTable("RolePermissions", "Security");
        }
    }
}
