#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects
{
    public sealed record CampaignName
    {
        public string Value { get; init; }

        public CampaignName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Campaign name cannot be empty.");
            Value = value;
        }
    }
}

#endif