using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Repositories
{
    public interface ICategoryAttributeRepository : IEntityRepository<CategoryAttribute>
    {
    }
}
