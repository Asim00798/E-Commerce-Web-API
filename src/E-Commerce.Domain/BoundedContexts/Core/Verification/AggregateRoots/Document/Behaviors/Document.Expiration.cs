using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Policies;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors
{
    public partial class Document
    {
        public void Expire(DateTime currentDate, DocumentExpirationPolicy policy)
        {
            if (Status != VerificationStatus.Approved)
                throw new Exceptions.DocumentException($"Only approved documents can expire. Current: {Status}.");
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (!policy.IsExpired(ExpirationDate, currentDate))
                throw new Exceptions.DocumentException("Document has not yet expired.");

            Status = VerificationStatus.Expired;
            ExpiredAt = DateTime.UtcNow;
            ExpirationReason = $"Expired automatically on {currentDate:yyyy-MM-dd}";

            AddDomainEvent(new DocumentExpiredEvent(Id, ExpirationReason));
        }

        public void AutoExpireIfNeeded(DateTime currentDate, DocumentExpirationPolicy policy)
        {
            if (Status == VerificationStatus.Approved && policy.IsExpired(ExpirationDate, currentDate))
            {
                Expire(currentDate, policy);
            }
        }

        public void ExpireDueToRestriction(Guid restrictionId, string restrictionDescription)
        {
            if (Status != VerificationStatus.Approved)
                throw new Exceptions.DocumentException($"Only approved documents can expire. Current: {Status}");

            Status = VerificationStatus.Expired;
            ExpiredAt = DateTime.UtcNow;
            ExpirationReason = $"Expired due to restriction '{restrictionDescription}' (RestrictionId: {restrictionId})";

            AddDomainEvent(new DocumentExpiredEvent(Id, ExpirationReason));
        }

        public void ExpireDueToDocumentExpiry(Guid documentId, string documentDescription)
        {
            if (Status != VerificationStatus.Approved)
                throw new Exceptions.DocumentException($"Only approved documents can expire. Current: {Status}");

            Status = VerificationStatus.Expired;
            ExpiredAt = DateTime.UtcNow;
            ExpirationReason = $"Expired because document '{documentDescription}' expired (DocumentId: {documentId})";

            AddDomainEvent(new DocumentExpiredEvent(Id, ExpirationReason));
        }
    }
}

