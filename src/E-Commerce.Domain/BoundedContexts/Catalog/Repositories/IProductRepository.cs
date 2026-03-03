using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.Catalog.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
    }
}
