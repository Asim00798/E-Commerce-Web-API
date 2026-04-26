using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;


namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors
{
    public partial class Document
    {
        public Document Renew(Guid submittedByUserId, IEnumerable<Guid> newFileIds)
        {
            if (Status != VerificationStatus.Approved && Status != VerificationStatus.Expired)
                throw new Exceptions.DocumentException($"Only approved or expired documents can be renewed. Current: {Status}.");
            if (newFileIds == null || !newFileIds.Any())
                throw new Exceptions.DocumentException("At least one file required for renewal.");

            var renewed = new Document(Owner, Type, newFileIds, submittedByUserId);
            renewed.PreviousVerificationId = Id;

            AddDomainEvent(new DocumentRenewedEvent(Id, renewed.Id, submittedByUserId));
            return renewed;
        }

        public void Resubmit(Guid submittedByUserId, IEnumerable<Guid> newFileIds)
        {
            if (Status != VerificationStatus.Rejected)
                throw new Exceptions.DocumentException($"Only rejected documents can be resubmitted. Current: {Status}.");
            if (newFileIds == null || !newFileIds.Any())
                throw new Exceptions.DocumentException("At least one file required for resubmission.");

            _fileIds.Clear();
            _fileIds.AddRange(newFileIds);
            Status = VerificationStatus.Draft;
            SubmittedByUserId = submittedByUserId;
            SubmittedAt = null;
            RejectedAt = null;
            RejectionReason = null;
            ReviewedByAdminId = null;

            AddDomainEvent(new DocumentResubmittedEvent(Id, submittedByUserId, newFileIds.ToList()));
        }
    }
}


