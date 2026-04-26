using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors
{
    public partial class Document
    {
        public bool IsVerified => Status == VerificationStatus.Approved && (!VerifiedUntil.HasValue || VerifiedUntil.Value > DateTime.UtcNow);
        public bool IsValidForCompliance(DateTime currentDate) => IsVerified && !ExpirationDate.IsExpired(currentDate);
    }
}

