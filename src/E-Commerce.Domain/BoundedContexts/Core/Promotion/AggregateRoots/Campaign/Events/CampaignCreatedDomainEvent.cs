#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Events
{
    public class CampaignCreatedDomainEvent : DomainEvent
    {
        public Guid CampaignId { get; }
        public string Name { get; }

        public CampaignCreatedDomainEvent(Guid campaignId, string name)
        {
            CampaignId = campaignId;
            Name = name;
        }
    }
}

#endif