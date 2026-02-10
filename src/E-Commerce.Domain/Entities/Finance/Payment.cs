using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.DomainEvents.Finance.Payment;

namespace E_Commerce.Domain.Entities.Finance
{
    public class Payment : BaseEntity
    {
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; } = PaymentMethod.CreditCard;
        public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
        public Currency Currency { get; private set; } = Currency.AED;
        public string? TransactionReference { get; private set; }
        public string? InvoiceNumber { get; private set; }

        // Navigation
        public User? User { get; private set; }

        // DDD Constructor
        public Payment(Guid userId, decimal amount, PaymentMethod paymentMethod, Currency currency, string invoiceNumber)
        {
            if (amount <= 0)
                throw new BusinessRuleViolationException("Payment amount must be positive.");

            UserId = userId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Currency = currency;
            InvoiceNumber = invoiceNumber;
            PaymentStatus = PaymentStatus.Pending;

            AddDomainEvent(new PaymentInitiated(Id));
        }

        public void Authorize()
        {
            if (PaymentStatus != PaymentStatus.Pending)
                throw new BusinessRuleViolationException("Only pending payments can be authorized.");

            PaymentStatus = PaymentStatus.Authorized;
            AddDomainEvent(new PaymentAuthorized(Id));
        }

        public void Capture()
        {
            if (PaymentStatus != PaymentStatus.Authorized)
                throw new BusinessRuleViolationException("Only authorized payments can be captured.");

            PaymentStatus = PaymentStatus.Captured;
            AddDomainEvent(new PaymentCaptured(Id));
        }

        public void Complete()
        {
            if (PaymentStatus != PaymentStatus.Captured && PaymentStatus != PaymentStatus.Pending)
                throw new BusinessRuleViolationException("Payment cannot be completed from current state.");

            PaymentStatus = PaymentStatus.Completed;
            AddDomainEvent(new PaymentCompleted(Id));
        }

        public void Cancel()
        {
            if (PaymentStatus == PaymentStatus.Completed || PaymentStatus == PaymentStatus.Settled)
                throw new BusinessRuleViolationException("Cannot cancel a completed or settled payment.");

            PaymentStatus = PaymentStatus.Cancelled;
            AddDomainEvent(new PaymentCancelled(Id));
        }

        public void Decline()
        {
            PaymentStatus = PaymentStatus.Declined;
            AddDomainEvent(new PaymentDeclined(Id));
        }

        public void Settle()
        {
            if (PaymentStatus != PaymentStatus.Completed)
                throw new BusinessRuleViolationException("Only completed payments can be settled.");

            PaymentStatus = PaymentStatus.Settled;
            AddDomainEvent(new PaymentSettled(Id));
        }

        public void MarkAsFraud()
        {
            AddDomainEvent(new PaymentMarkedAsFraud(Id));
        }

        public override void Validate()
        {
            base.Validate();

            if (Amount <= 0)
                throw new InvalidOperationException("Payment amount must be positive.");
        }
    }
}
