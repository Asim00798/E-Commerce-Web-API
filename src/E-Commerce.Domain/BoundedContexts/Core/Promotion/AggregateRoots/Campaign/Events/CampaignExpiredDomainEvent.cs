#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Events
{
    public class CampaignExpiredDomainEvent : DomainEvent
    {
        public Guid CampaignId { get; }

        public CampaignExpiredDomainEvent(Guid campaignId)
        {
            CampaignId = campaignId;
        }
    }
}

#endif