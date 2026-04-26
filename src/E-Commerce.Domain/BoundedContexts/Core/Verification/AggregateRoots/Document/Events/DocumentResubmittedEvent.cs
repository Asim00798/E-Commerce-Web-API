using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentResubmittedEvent : DomainEvent
{
    public Guid DocumentId { get; }
    public Guid SubmittedByUserId { get; }
    public IReadOnlyList<Guid> NewFileIds { get; }
    public DocumentResubmittedEvent(Guid documentId, Guid submittedByUserId, IReadOnlyList<Guid> newFileIds)
        => (DocumentId, SubmittedByUserId, NewFileIds) = (documentId, submittedByUserId, newFileIds);
}
