using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Profiles;
using E_Commerce.Domain.Entities.Ordering;

namespace E_Commerce.Application.ApplicationEvents.Reporting.Reporting
{
    public class CustomerActivity : BaseEntity
    {
        public Guid CustomerProfileId { get; set; }
        public CustomerProfile CustomerProfile { get; set; } = null!;

        public int TotalOrders { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0m;
        public int TotalReviews { get; set; } = 0;

        // Optional: last activity date
        public DateTimeOffset? LastOrderDate { get; set; }
        public DateTimeOffset? LastReviewDate { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (CustomerProfileId == Guid.Empty)
                throw new InvalidOperationException("CustomerActivity must be linked to a CustomerProfile.");

            if (TotalOrders < 0 || TotalSpent < 0 || TotalReviews < 0)
                throw new InvalidOperationException("CustomerActivity metrics cannot be negative.");
        }
    }
}
