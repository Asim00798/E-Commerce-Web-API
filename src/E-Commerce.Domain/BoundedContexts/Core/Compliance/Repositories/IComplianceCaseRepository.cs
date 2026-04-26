using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase;
using E_Commerce.Domain.SharedKernel.Persistence;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Repositories
{
    public interface IComplianceCaseRepository : IRepository<ComplianceCase>
    {
        // logic specific to ComplianceCase retrieval can be added here, e.g.:
    }
}
