using E_Commerce.Application.Common.Abstractions.Identity;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Identity.Entities;
using E_Commerce.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace E_Commerce.Infrastructure.Persistence.Context;
/// <summary>
/// Unified EF Core DbContext for all write-side operations.
/// Ensures strong consistency and a single transaction boundary across bounded contexts.
/// </summary>
public partial class AppDbContext : IdentityDbContext<User, Role, Guid>
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
        modelBuilder.RenameIdentityTables();
    }
}
