using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentReviewStartedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public Guid AdminId { get; }
    public DocumentReviewStartedEvent(Guid documentId, Guid adminId) => (DocumentId, AdminId) = (documentId, adminId);
}
