using System.Linq.Expressions;
using ProductAggregate = E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors.Product;
using E_Commerce.Domain.SharedKernel.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Specifications.Internal;

/// <summary>
/// Internal specification to filter products that are discontinued.
/// Used for queries (e.g., admin view of discontinued products).
/// </summary>
public sealed class DiscontinuedProductSpecification : ISpecification<ProductAggregate>
{
    public Expression<Func<ProductAggregate, bool>> ToExpression()
        => product => product.Status == ProductStatus.Discontinued;

    public bool IsSatisfiedBy(ProductAggregate entity)
        => entity.Status == ProductStatus.Discontinued;
}