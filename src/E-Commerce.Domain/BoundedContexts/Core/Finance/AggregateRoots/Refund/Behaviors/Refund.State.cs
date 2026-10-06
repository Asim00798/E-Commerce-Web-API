using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors
{
    public partial class Refund
    {
        public void MarkProcessing()
        {
            if (Status != RefundStatus.Requested)
                throw new RefundException("Refund can only be marked processing from requested state.");

            Status = RefundStatus.Processing;
        }

        public void Complete()
        {
            if (Status != RefundStatus.Processing)
                throw new RefundException("Refund can only be completed from processing state.");

            Status = RefundStatus.Completed;
            CompletedAtUtc = DateTime.UtcNow;

            AddDomainEvent(new RefundCompletedDomainEvent(
                Id,
                PaymentId,
                OrderId,
                Amount));
        }

        public void Fail(string? reason = null)
        {
            if (Status != RefundStatus.Processing)
                throw new RefundException("Refund can only be failed from processing state.");

            Status = RefundStatus.Failed;

            AddDomainEvent(new RefundFailedDomainEvent(
                Id,
                PaymentId,
                OrderId,
                Amount,
                reason));
        }

        /// <summary>
        /// Recovers a stuck Processing refund back to Requested so it can be retried.
        /// This is a legitimate recovery transition, not a business outcome.
        /// </summary>
        public void Requeue()
        {
            if (Status != RefundStatus.Processing)
                throw new RefundException("Only a processing refund can be requeued.");

            Status = RefundStatus.Requested;
        }

        public void SetProviderTransactionId(string providerTransactionId)
        {
            if (string.IsNullOrWhiteSpace(providerTransactionId))
                throw new RefundException("Provider transaction ID is required.");

            ProviderTransactionId = providerTransactionId;
        }
    }
}
