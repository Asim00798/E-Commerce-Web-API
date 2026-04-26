#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Entities
{
    public class ListingModeration : BaseEntity
    {
        public ModerationStatus Status { get; private set; }

        public ListingModeration()
        {
            Status = ModerationStatus.Pending;
        }

        public void Approve(string remarks)
        {
            Status = new ModerationStatus(true, remarks, DateTime.UtcNow);
        }

        public void Reject(string remarks)
        {
            Status = new ModerationStatus(false, remarks, DateTime.UtcNow);
        }
    }
}

#endif