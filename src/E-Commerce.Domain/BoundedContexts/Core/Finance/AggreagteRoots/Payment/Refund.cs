#if false
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.Events.Finance.Refund;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Payment
{
    public class Refund : BaseEntity
    {
        public Guid PaymentId { get; private set; }
        public Money Amount { get; private set; }
        public string? Reason { get; private set; }
        public RefundStatus Status { get; private set; } = RefundStatus.Requested;
        public DateTimeOffset RefundDate { get; private set; } = DateTimeOffset.UtcNow;

        // Navigation
        public Payment? Payment { get; private set; }

        // DDD Constructor
        internal Refund(Guid paymentId, Money amount, string? reason)
        {

            PaymentId = paymentId;
            Amount = amount;
            Reason = reason;
            Status = RefundStatus.Requested;
            RefundDate = DateTimeOffset.UtcNow;

            AddDomainEvent(new RefundRequested(Id));
        }

        public void Approve()
        {
            if (Status != RefundStatus.Requested)
                throw new BusinessRuleViolationException("Only requested refunds can be approved.");

            Status = RefundStatus.Approved;
            AddDomainEvent(new RefundApproved(Id));
        }

        public void Reject()
        {
            if (Status != RefundStatus.Requested)
                throw new BusinessRuleViolationException("Only requested refunds can be rejected.");

            Status = RefundStatus.Rejected;
            AddDomainEvent(new RefundRejected(Id));
        }

        public void Complete()
        {
            if (Status != RefundStatus.Approved)
                throw new BusinessRuleViolationException("Only approved refunds can be completed.");

            Status = RefundStatus.Completed;
            AddDomainEvent(new RefundCompleted(Id));
        }

        public void Cancel()
        {
            if (Status == RefundStatus.Completed)
                throw new BusinessRuleViolationException("Cannot cancel a completed refund.");

            Status = RefundStatus.Cancelled;
            AddDomainEvent(new RefundCancelled(Id));
        }

        public void Fail()
        {
            Status = RefundStatus.Failed;
            AddDomainEvent(new RefundFailed(Id));
        }

        public override void Validate()
        {
            base.Validate();

            if (Amount.Amount <= 0)
                throw new InvalidOperationException("Refund amount must be positive.");
        }
    }
}

#endif