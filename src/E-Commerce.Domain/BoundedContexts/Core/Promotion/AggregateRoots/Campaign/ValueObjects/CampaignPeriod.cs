#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects
{
    public sealed record CampaignPeriod
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

        public CampaignPeriod(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date must be after start date.");
            StartDate = startDate;
            EndDate = endDate;
        }

        public bool IsInside(DateTime date) => date >= StartDate && date <= EndDate;
    }
}

#endif