using E_Commerce.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
