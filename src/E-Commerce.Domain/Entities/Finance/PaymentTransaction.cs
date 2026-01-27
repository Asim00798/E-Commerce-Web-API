using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.Finance
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;
        public string? TransactionReference { get; set; }

        // Navigation
        public Payment? Payment { get; set; }
    }
}
