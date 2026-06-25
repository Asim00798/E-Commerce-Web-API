
using E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        // Auditlog
        public DbSet<AuditLog> AuditLog { get; set; }
    }
}
