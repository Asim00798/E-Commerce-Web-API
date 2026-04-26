#if false
using System.Linq.Expressions;
using E_Commerce.Domain.SharedKernel.Specifications;
using ListingAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Behaviors.MarketplaceListing;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Specifications
{
    public class ListingsBySellerSpecification : ISpecification<ListingAggregate>
    {
        private readonly Guid _sellerId;

        public ListingsBySellerSpecification(Guid sellerId)
        {
            _sellerId = sellerId;
        }

        public Expression<Func<ListingAggregate, bool>> ToExpression()
        {
            return listing => listing.SellerId.Value == _sellerId;
        }

        public bool IsSatisfiedBy(ListingAggregate entity)
        {
            return entity.SellerId.Value == _sellerId;
        }
    }
}

#endif