using E_Commerce.Domain.ContextBounded.Catalog.AggregateRoots.Brand.Entities;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Brand.Repositories
{
    public interface IBrandRestrictionRepository : IEntityRepository<BrandRestriction>
    {
    }
}
