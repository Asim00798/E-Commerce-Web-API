using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;
using E_Commerce.Domain.Enums;
using E_Commerce.Domain.ValueObjects;

namespace E_Commerce.Domain.Entities.Finance
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid PaymentId { get; private set; }
        public Money Amount { get; private set; }
        public PaymentMethod Method { get; private set; }
        public DateTimeOffset OccurredAt { get; private set; }
        public string? GatewayReference { get; private set; }

        // Navigation
        public Payment? Payment { get; private set; }

        /*If an entity is not an Aggregate Root,
          its constructor must not be public.*/
        internal PaymentTransaction(
            Guid paymentId,
            Money amount,
            PaymentMethod method,
            string? gatewayReference = null)
        {
            PaymentId = paymentId;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            Method = method;
            GatewayReference = gatewayReference;
            OccurredAt = DateTimeOffset.UtcNow;
        }
    }

}
