using E_Commerce.Infrastructure.Persistence.Audit;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
