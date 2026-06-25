using E_Commerce.Domain.SharedKernel.Abstractions;
namespace E_Commerce.Infrastructure.Persistence.Extensions
{
    public static class ValidationExtensions
    {
        public static void ValidateEntities(this DbContext context)
        {
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                entry.Entity.Validate();
            }
        }
    }
}
