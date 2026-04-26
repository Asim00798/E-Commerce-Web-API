using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentExpiredEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public string Reason { get; }
    public DocumentExpiredEvent(Guid documentId, string reason) => (DocumentId, Reason) = (documentId, reason);
}
