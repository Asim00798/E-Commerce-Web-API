
using E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.Entities;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.Repositories
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {}
}
