#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Rules
{
    public class ListingMustBeApprovedRule : IBusinessRule
    {
        private readonly ModerationStatus _status;

        public ListingMustBeApprovedRule(ModerationStatus status)
        {
            _status = status;
        }

        public bool IsSatisfied() => _status.IsApproved;

        public string Message => "Listing must be approved by a moderator before activation.";
    }
}

#endif