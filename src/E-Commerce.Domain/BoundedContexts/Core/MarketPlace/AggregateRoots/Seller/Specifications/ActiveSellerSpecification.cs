#if false
using System.Linq.Expressions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Enums;
using E_Commerce.Domain.SharedKernel.Specifications;
using SellerAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Behaviors.Seller;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Specifications
{
    public class ActiveSellerSpecification : ISpecification<SellerAggregate>
    {
        public Expression<Func<SellerAggregate, bool>> ToExpression()
        {
            return seller => seller.Status == SellerStatusEnum.Active;
        }

        public bool IsSatisfiedBy(SellerAggregate entity)
        {
            return entity.Status == SellerStatusEnum.Active;
        }
    }
}

#endif