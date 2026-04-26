#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Rules
{
    public class StoreMustBelongToSellerRule : IBusinessRule
    {
        private readonly SellerId _actualSellerId;
        private readonly SellerId _expectedSellerId;

        public StoreMustBelongToSellerRule(SellerId actualSellerId, SellerId expectedSellerId)
        {
            _actualSellerId = actualSellerId;
            _expectedSellerId = expectedSellerId;
        }

        public bool IsSatisfied() => _actualSellerId == _expectedSellerId;

        public string Message => "Storefront must belong to the specified seller.";
    }
}

#endif