#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Campaign.Entities
{
    public class CampaignUsage : BaseEntity
    {
        public Guid CustomerId { get; private set; }
        public Guid OrderId { get; private set; }
        public DateTime UsedAt { get; private set; }

        public CampaignUsage(Guid customerId, Guid orderId)
        {
            CustomerId = customerId;
            OrderId = orderId;
            UsedAt = DateTime.UtcNow;
        }
    }
}

#endif