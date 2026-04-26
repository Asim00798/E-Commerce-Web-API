#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Events
{
    public class CampaignActivatedDomainEvent : DomainEvent
    {
        public Guid CampaignId { get; }

        public CampaignActivatedDomainEvent(Guid campaignId)
        {
            CampaignId = campaignId;
        }
    }
}

#endif