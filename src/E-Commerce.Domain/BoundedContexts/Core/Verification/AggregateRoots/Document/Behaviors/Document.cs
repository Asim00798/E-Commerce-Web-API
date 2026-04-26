using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Policies;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors;

public partial class Document : BaseEntity, IAggregateRoot
{
    private static readonly TimeSpan DefaultValidityPeriod = TimeSpan.FromDays(365);

    // Core
    public DocumentOwner Owner { get; private set; } = null!;
    public DocumentType Type { get; private set; } = null!;
    public VerificationStatus Status { get; private set; }

    // Timestamps
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ReviewStartedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    // Expiration
    public DateTime? VerifiedUntil { get; private set; }
    public ExpirationDate ExpirationDate { get; private set; } = null!;

    // Files
    private readonly List<Guid> _fileIds = new();
    public IReadOnlyList<Guid> FileIds => _fileIds;

    // Audit
    public Guid? SubmittedByUserId { get; private set; }
    public Guid? ReviewedByAdminId { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? ExpirationReason { get; private set; }

    // Renewal chain
    public Guid? PreviousVerificationId { get; private set; }

    // History
    private readonly List<VerificationRecord> _history = new();
    public IReadOnlyCollection<VerificationRecord> History => _history.AsReadOnly();

    private Document(DocumentOwner owner, DocumentType type, IEnumerable<Guid> fileIds, Guid? submittedByUserId)
    {
        Id = Guid.NewGuid();
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Status = VerificationStatus.Draft;
        ExpirationDate = ExpirationDate.On(DateTime.UtcNow.Add(DefaultValidityPeriod));
        _fileIds.AddRange(fileIds);
        SubmittedByUserId = submittedByUserId;
    }

    public static Document Create(DocumentOwner owner, DocumentType type, IEnumerable<Guid> fileIds, DocumentTypePolicy typePolicy, Guid? submittedByUserId = null)
    {
        if (fileIds == null || !fileIds.Any())
            throw new Exceptions.DocumentException("At least one file is required.");
        
        if (typePolicy == null)
            throw new Exceptions.DocumentException("DocumentTypePolicy is required to validate the document type.");

        if (!typePolicy.IsAllowed(type))
            throw new Exceptions.DocumentException($"'{type.Value}' is not an allowed document type per system policy.");

        return new Document(owner, type, fileIds, submittedByUserId);
    }
}