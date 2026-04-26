#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects
{
    public sealed record CouponPeriod
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

        public CouponPeriod(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date must be after start date.");
            StartDate = startDate;
            EndDate = endDate;
        }

        public bool IsActive(DateTime date) => date >= StartDate && date <= EndDate;
    }
}

#endif