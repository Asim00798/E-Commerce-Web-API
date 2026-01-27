using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Refund : BaseEntity
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset RefundDate { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public Payment? Payment { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Amount <= 0)
                throw new InvalidOperationException("Refund amount must be positive.");
        }
    }
}
