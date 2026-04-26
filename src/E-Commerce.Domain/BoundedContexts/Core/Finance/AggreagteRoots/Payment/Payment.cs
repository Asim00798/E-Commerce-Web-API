#if false
using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.Events.Finance.Payment;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment
{
    public class Payment : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Money Amount { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public PaymentStatus PaymentStatus { get; private set; }
        public string? TransactionReference { get; private set; }
        public string InvoiceNumber { get; private set; }

        // Navigation
        public User? User { get; private set; }

        // DDD Constructor
        public Payment(Guid userId, Money amount, PaymentMethod method, string invoiceNumber)
        {
            UserId = userId;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            PaymentMethod = method;
            InvoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
            PaymentStatus = PaymentStatus.Pending;

            AddDomainEvent(new PaymentInitiated(Id));
        }

        // Behavior methods
        public void Authorize()
        {
            Ensure(PaymentStatus == PaymentStatus.Pending, "Only pending payments can be authorized.");
            PaymentStatus = PaymentStatus.Authorized;
            AddDomainEvent(new PaymentAuthorized(Id));
        }

        public void Capture()
        {
            Ensure(PaymentStatus == PaymentStatus.Authorized, "Only authorized payments can be captured.");
            PaymentStatus = PaymentStatus.Captured;
            AddDomainEvent(new PaymentCaptured(Id));
        }

        public void Complete()
        {
            Ensure(PaymentStatus == PaymentStatus.Captured, "Only captured payments can be completed.");
            PaymentStatus = PaymentStatus.Completed;
            AddDomainEvent(new PaymentCompleted(Id));
        }

        public void Settle()
        {
            Ensure(PaymentStatus == PaymentStatus.Completed, "Only completed payments can be settled.");
            PaymentStatus = PaymentStatus.Settled;
            AddDomainEvent(new PaymentSettled(Id));
        }

        public void Cancel()
        {
            Ensure(PaymentStatus == PaymentStatus.Pending || PaymentStatus == PaymentStatus.Authorized,
                   "Only pending or authorized payments can be cancelled.");

            PaymentStatus = PaymentStatus.Cancelled;
            AddDomainEvent(new PaymentCancelled(Id));
        }

        public void Decline()
        {
            Ensure(PaymentStatus == PaymentStatus.Pending || PaymentStatus == PaymentStatus.Authorized,
                   "Only pending or authorized payments can be declined.");

            PaymentStatus = PaymentStatus.Declined;
            AddDomainEvent(new PaymentDeclined(Id));
        }

        public void MarkAsFraud()
        {
            AddDomainEvent(new PaymentMarkedAsFraud(Id));
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition) throw new BusinessRuleViolationException(message);
        }
    }

}

#endif