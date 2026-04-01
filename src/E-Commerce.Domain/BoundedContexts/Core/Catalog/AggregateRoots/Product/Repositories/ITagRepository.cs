using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Repositories
{
    public interface ITagRepository : IEntityRepository<Tag>
    {
    }
}
