using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Events;

public sealed class DocumentRenewedEvent : DomainEvent
{
    public Guid OldDocumentId { get; }
    public Guid NewDocumentId { get; }
    public Guid SubmittedByUserId { get; }
    public DocumentRenewedEvent(Guid oldDocumentId, Guid newDocumentId, Guid submittedByUserId)
        => (OldDocumentId, NewDocumentId, SubmittedByUserId) = (oldDocumentId, newDocumentId, submittedByUserId);
}
