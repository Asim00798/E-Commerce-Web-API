using System;

namespace E_Commerce.Domain.Catalog.AggregateRoots.Brand.Policies
{
    public class BrandPayoutSchedulePolicy
    {
        public DateTime GetNextPayoutDate(DateTime lastPayout)
        {
            // Business Logic: Calculate the next payout date based on the brand's selected payout frequency (e.g., weekly, monthly).
            return lastPayout.AddDays(30);
        }
    }
}
