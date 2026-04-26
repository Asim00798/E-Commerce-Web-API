#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceCampaign.Events
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