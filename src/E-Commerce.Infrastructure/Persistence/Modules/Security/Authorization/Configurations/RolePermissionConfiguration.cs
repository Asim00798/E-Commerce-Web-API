using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Security.Authorization.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RolePermission"/> entity.
/// </summary>
internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "security");

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // FK to built-in Identity role table (no navigation property)
        builder.HasOne<IdentityRole<Guid>>()
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}