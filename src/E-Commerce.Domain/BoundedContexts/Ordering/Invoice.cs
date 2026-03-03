using E_Commerce.Domain.BoundedContexts.Ordering;
using E_Commerce.Domain.BoundedContexts.Finance;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Ordering
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid? PaymentId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
        public decimal Amount { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public Payment? Payment { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(InvoiceNumber))
                throw new InvalidOperationException("InvoiceNumber cannot be empty.");

            if (Amount < 0)
                throw new InvalidOperationException("Invoice amount cannot be negative.");
        }
    }
}
