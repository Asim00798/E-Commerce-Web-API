using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Enums;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Payment : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public Currency Currency { get; set; } = Currency.AED;
        public string? TransactionReference { get; set; }
        public string? InvoiceNumber { get; set; }

        // Navigation
        public User? User { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Amount <= 0)
                throw new InvalidOperationException("Payment amount must be positive.");
        }
    }
}
