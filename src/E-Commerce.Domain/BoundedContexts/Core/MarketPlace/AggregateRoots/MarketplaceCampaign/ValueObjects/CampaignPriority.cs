#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.ValueObjects
{
    public sealed record CampaignPriority
    {
        public int Value { get; init; }

        public CampaignPriority(int value)
        {
            Value = value;
        }

        public static CampaignPriority Low => new(1);
        public static CampaignPriority Medium => new(5);
        public static CampaignPriority High => new(10);
    }
}

#endif