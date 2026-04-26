#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Enums;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Rules
{
    public class SellerMustBeActiveRule : IBusinessRule
    {
        private readonly SellerStatusEnum _status;

        public SellerMustBeActiveRule(SellerStatusEnum status)
        {
            _status = status;
        }

        public bool IsSatisfied() => _status == SellerStatusEnum.Active;

        public string Message => "Seller must be in Active status to list or activate products.";
    }
}

#endif