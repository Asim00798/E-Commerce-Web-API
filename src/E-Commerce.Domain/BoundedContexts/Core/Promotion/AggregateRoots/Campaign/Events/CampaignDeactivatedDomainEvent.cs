#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Events
{
    public class CampaignDeactivatedDomainEvent : DomainEvent
    {
        public Guid CampaignId { get; }
        public string Reason { get; }

        public CampaignDeactivatedDomainEvent(Guid campaignId, string reason)
        {
            CampaignId = campaignId;
            Reason = reason;
        }
    }
}

#endif