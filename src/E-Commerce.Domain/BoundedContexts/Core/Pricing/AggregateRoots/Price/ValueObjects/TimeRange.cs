#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record TimeRange
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public TimeRange(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End date must be after start date.");

            Start = start;
            End = end;
        }

        public bool IsActive(DateTime current) => current >= Start && current <= End;
        public bool IsActiveNow => IsActive(DateTime.UtcNow);
    }
}

#endif