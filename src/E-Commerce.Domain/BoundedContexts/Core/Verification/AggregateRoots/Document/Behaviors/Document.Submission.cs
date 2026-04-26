using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors
{
    public partial class Document
    {
        public void SubmitForReview()
        {
            if (Status != VerificationStatus.Draft)
                throw new Exceptions.DocumentException($"Cannot submit when status is {Status}.");
            if (_fileIds.Count == 0)
                throw new Exceptions.DocumentException("Cannot submit without files.");

            Status = VerificationStatus.PendingReview;
            SubmittedAt = DateTime.UtcNow;

            AddDomainEvent(new DocumentSubmittedEvent(Id, Owner, Type, _fileIds.ToList(), SubmittedByUserId));
        }
    }
}

