#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.ValueObjects
{
    public sealed record CampaignStatus
    {
        public CampaignStatusEnum Value { get; init; }

        public CampaignStatus(CampaignStatusEnum value)
        {
            Value = value;
        }

        public static CampaignStatus Draft => new(CampaignStatusEnum.Draft);
        public static CampaignStatus Active => new(CampaignStatusEnum.Active);
    }
}

#endif