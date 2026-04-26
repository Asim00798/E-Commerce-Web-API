#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Rules
{
    public class SellerMustBeVerifiedRule : IBusinessRule
    {
        private readonly VerificationStatus _status;

        public SellerMustBeVerifiedRule(VerificationStatus status)
        {
            _status = status;
        }

        public bool IsSatisfied() => _status.IsVerified;

        public string Message => "Seller must be verified to perform this action.";
    }
}

#endif