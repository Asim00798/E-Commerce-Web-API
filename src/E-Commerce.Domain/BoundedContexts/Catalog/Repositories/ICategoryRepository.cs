using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Category.Behaviors;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.Catalog.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
    }
}
