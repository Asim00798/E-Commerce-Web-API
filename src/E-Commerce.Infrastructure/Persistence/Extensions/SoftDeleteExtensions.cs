
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Infrastructure.Persistence.Extensions
{
    public static class SoftDeleteExtensions
    {
        public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query) where T : BaseEntity
        {
            return query.Where(e => !e.IsDeleted);
        }

        public static void SoftDelete<T>(this T entity) where T : BaseEntity
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
        }

        public static void Restore<T>(this T entity) where T : BaseEntity
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
        }

        /// <summary>
        /// Applies soft delete query filter for all entities inheriting BaseEntity.
        /// </summary>
        public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
        {
            var baseEntityType = typeof(BaseEntity);

            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(t => baseEntityType.IsAssignableFrom(t.ClrType));

            foreach (var entityType in entityTypes)
            {
                var method = typeof(SoftDeleteExtensions)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Intercepts hard deletes in ChangeTracker and turns them into soft deletes.
        /// </summary>
        public static void ApplySoftDelete(this DbContext context)
        {
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.State = EntityState.Modified; // prevent physical delete
                }
            }
        }
    }
}
