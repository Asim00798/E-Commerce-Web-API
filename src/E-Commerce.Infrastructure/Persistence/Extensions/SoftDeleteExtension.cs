using E_Commerce.Domain.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Extensions;

public static class SoftDeleteExtension
{
    /// <summary>
    /// Applies a global query filter to entities inheriting BaseEntity.
    /// </summary>
    public static void ApplySoftDeleteFilter(
        this ModelBuilder modelBuilder)
    {
        var baseEntityType = typeof(BaseEntity);

        var entityTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => baseEntityType.IsAssignableFrom(t.ClrType));

        foreach (var entityType in entityTypes)
        {
            var method = typeof(SoftDeleteExtension)
                .GetMethod(
                    nameof(SetSoftDeleteFilter),
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static)!;

            method = method.MakeGenericMethod(entityType.ClrType);

            method.Invoke(
                null,
                new object[] { modelBuilder });
        }
    }

    private static void SetSoftDeleteFilter<T>(
        ModelBuilder modelBuilder)
        where T : BaseEntity
    {
        modelBuilder
            .Entity<T>()
            .HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Converts hard-delete requests for BaseEntity instances
    /// into soft deletes.
    /// </summary>
    public static void ApplySoftDelete(
        this DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State != EntityState.Deleted)
                continue;

            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;

            // Prevent physical DELETE.
            entry.State = EntityState.Modified;
        }
    }
}