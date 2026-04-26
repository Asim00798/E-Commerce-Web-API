#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Entities
{
    public class SellerVerification : BaseEntity
    {
        public string VerificationType { get; private set; }
        public bool IsSuccessful { get; private set; }
        public DateTime PerformedAt { get; private set; }
        public string Observations { get; private set; }

        public SellerVerification(string verificationType, bool isSuccessful, string observations)
        {
            VerificationType = verificationType;
            IsSuccessful = isSuccessful;
            Observations = observations;
            PerformedAt = DateTime.UtcNow;
        }
    }
}

#endif