using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;
using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;

using E_Commerce.Domain.BoundedContexts.Core.Verification.Policies;


namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors
{
    public partial class Document
    {
        public void StartReview(Guid adminId)
        {
            if (adminId == Guid.Empty)
                throw new Exceptions.DocumentException("AdminId required.");
            if (Status != VerificationStatus.PendingReview)
                throw new Exceptions.DocumentException($"Cannot start review when status is {Status}.");

            Status = VerificationStatus.UnderReview;
            ReviewStartedAt = DateTime.UtcNow;
            ReviewedByAdminId = adminId;

            AddDomainEvent(new DocumentReviewStartedEvent(Id, adminId));
        }

        public void Approve(Guid adminId, VerificationMethod method, ExpirationDate? customExpiration = null, VerificationMetadata? metadata = null, DocumentApprovalPolicy? policy = null)
        {
            if (adminId == Guid.Empty)
                throw new Exceptions.DocumentException("AdminId required.");
            if (Status != VerificationStatus.PendingReview && Status != VerificationStatus.UnderReview)
                throw new Exceptions.DocumentException($"Cannot approve when status is {Status}.");

            policy ??= new DocumentApprovalPolicy();
            if (!policy.IsAllowedToApprove(this, adminId))
                throw new Exceptions.DocumentException("Approval policy rejected this action.");

            Status = VerificationStatus.Approved;
            ApprovedAt = DateTime.UtcNow;
            ReviewedByAdminId = adminId;
            VerifiedUntil = customExpiration?.Date ?? DateTime.UtcNow.Add(DefaultValidityPeriod);
            ExpirationDate = customExpiration ?? ExpirationDate.On(VerifiedUntil.Value);

            var record = new VerificationRecord(adminId, method, VerificationStatus.Approved, metadata);
            _history.Add(record);

            AddDomainEvent(new DocumentApprovedEvent(Id, Owner, Type, VerifiedUntil.Value, adminId));
        }

        public void Reject(Guid adminId, VerificationMethod method, string reason, VerificationMetadata? metadata = null)
        {
            if (adminId == Guid.Empty)
                throw new Exceptions.DocumentException("AdminId required.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new Exceptions.DocumentException("Rejection reason required.");
            if (Status != VerificationStatus.PendingReview && Status != VerificationStatus.UnderReview)
                throw new Exceptions.DocumentException($"Cannot reject when status is {Status}.");

            Status = VerificationStatus.Rejected;
            RejectedAt = DateTime.UtcNow;
            ReviewedByAdminId = adminId;
            RejectionReason = reason;

            var record = new VerificationRecord(adminId, method, VerificationStatus.Rejected, metadata, reason);
            _history.Add(record);

            AddDomainEvent(new DocumentRejectedEvent(Id, reason, adminId));
        }
    }
}

