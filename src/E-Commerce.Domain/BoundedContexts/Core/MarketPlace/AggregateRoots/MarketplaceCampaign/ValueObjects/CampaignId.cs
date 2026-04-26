#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.ValueObjects
{
    public sealed record CampaignId
    {
        public Guid Value { get; init; }

        public CampaignId(Guid value)
        {
            Value = value;
        }

        public static CampaignId New() => new(Guid.NewGuid());
    }
}

#endif