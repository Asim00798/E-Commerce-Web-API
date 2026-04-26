#if false
using System.Linq.Expressions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects;
using E_Commerce.Domain.SharedKernel.Specifications;
using ListingAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Behaviors.MarketplaceListing;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Specifications
{
    public class ActiveListingSpecification : ISpecification<ListingAggregate>
    {
        public Expression<Func<ListingAggregate, bool>> ToExpression()
        {
            return listing => listing.Status == ListingStatus.Active;
        }

        public bool IsSatisfiedBy(ListingAggregate entity)
        {
            return entity.Status == ListingStatus.Active;
        }
    }
}

#endif