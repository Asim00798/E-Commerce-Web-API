using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentRejectedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public string Reason { get; }
    public Guid AdminId { get; }
    public DocumentRejectedEvent(Guid documentId, string reason, Guid adminId) => (DocumentId, Reason, AdminId) = (documentId, reason, adminId);
}
