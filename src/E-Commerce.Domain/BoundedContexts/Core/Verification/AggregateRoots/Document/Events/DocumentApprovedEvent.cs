using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentApprovedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public DocumentOwner Owner { get; }
    public DocumentType Type { get; }
    public DateTime VerifiedUntil { get; }
    public Guid AdminId { get; }
    public DocumentApprovedEvent(Guid documentId, DocumentOwner owner, DocumentType type, DateTime verifiedUntil, Guid adminId)
        => (DocumentId, Owner, Type, VerifiedUntil, AdminId) = (documentId, owner, type, verifiedUntil, adminId);
}
