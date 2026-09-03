using E_Commerce.Infrastructure.Persistence.Extensions;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace E_Commerce.Infrastructure.Persistence.Context;
/// <summary>
/// Unified EF Core DbContext for all write-side operations.
/// Ensures strong consistency and a single transaction boundary across bounded contexts.
/// </summary>
public partial class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {} 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply soft-delete filters for all BaseEntity-derived entities
        modelBuilder.ApplySoftDeleteFilter();

        // Rename Identity Tables
        modelBuilder.RenameSecurityTables();
    }
}
