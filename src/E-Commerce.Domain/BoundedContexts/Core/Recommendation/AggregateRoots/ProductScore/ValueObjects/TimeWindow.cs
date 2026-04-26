#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.ValueObjects
{
    public sealed record TimeWindow
    {
        public TimeSpan Duration { get; init; }

        public TimeWindow(TimeSpan duration)
        {
            Duration = duration;
        }

        public static TimeWindow Past24Hours => new(TimeSpan.FromHours(24));
        public static TimeWindow Past7Days => new(TimeSpan.FromDays(7));
        public static TimeWindow Past30Days => new(TimeSpan.FromDays(30));
    }
}

#endif