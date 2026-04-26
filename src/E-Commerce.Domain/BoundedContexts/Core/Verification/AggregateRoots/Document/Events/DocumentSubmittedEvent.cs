using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentSubmittedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public DocumentOwner Owner { get; }
    public DocumentType Type { get; }
    public IReadOnlyList<Guid> FileIds { get; }
    public Guid? SubmittedByUserId { get; }

    public DocumentSubmittedEvent(Guid documentId, DocumentOwner owner, DocumentType type, IReadOnlyList<Guid> fileIds, Guid? submittedByUserId)
    {
        DocumentId = documentId;
        Owner = owner;
        Type = type;
        FileIds = fileIds;
        SubmittedByUserId = submittedByUserId;
    }
}
