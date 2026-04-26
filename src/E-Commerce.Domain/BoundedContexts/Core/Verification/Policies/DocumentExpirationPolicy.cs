using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.Policies
{
    /// <summary>
    /// Pure domain policy determining when an expiration date has lapsed.
    /// Extracts the temporal logic from the aggregate.
    /// </summary>
    public class DocumentExpirationPolicy
    {
        public virtual bool IsExpired(ExpirationDate expirationDate, DateTime currentDate)
        {
            if (expirationDate == null || expirationDate.DoesNotExpire)
                return false;

            return expirationDate.IsExpired(currentDate);
        }
    }
}
