#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Rules
{
    public class CampaignMustBeWithinValidPeriodRule : IBusinessRule
    {
        private readonly CampaignPeriod _period;
        private readonly DateTime _currentDate;

        public CampaignMustBeWithinValidPeriodRule(CampaignPeriod period, DateTime currentDate)
        {
            _period = period;
            _currentDate = currentDate;
        }

        public bool IsSatisfied() => _period.IsInside(_currentDate);

        public string Message => "Campaign is not within its valid period.";
    }
}

#endif