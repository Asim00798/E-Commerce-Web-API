using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.Enums;
using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog
{
    public class Document : BaseEntity, IAggregateRoot
    {
        public string FileName { get; private set; } = null!;
        public string Url { get; private set; } = null!;
        public DocumentType DocumentType { get; private set; }
        public string DocumentNumber { get; private set; } = null!;
        public Money? AssociatedValue { get; private set; }
        public DateTime IssuedDate { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public DocumentStatus Status { get; private set; }
        public int Version { get; private set; }
        public Guid? ReplacesDocumentId { get; private set; }

        // Owner reference (renamed from BrandId to be generic)
        public Guid OwnerId { get; private set; }
        public OwnerType OwnerType { get; private set; } // "Brand", "Employee", etc.

        // Factory method with audit info
        public static Document Create(
            string fileName,
            string url,
            DocumentType documentType,  // Now using enum directly
            string documentNumber,
            DateTime issuedDate,
            Guid ownerId,
            OwnerType ownerType,
            string createdBy,
            DateTime? expiryDate = null,
            Money? associatedValue = null)
        {
            // Business rules
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid document URL.", nameof(url));

            if (string.IsNullOrWhiteSpace(documentNumber))
                throw new ArgumentException("Document number required", nameof(documentNumber));

            if (expiryDate.HasValue && expiryDate <= issuedDate)
                throw new ArgumentException("Expiry must be after issue date");

            if (ownerId == Guid.Empty)
                throw new ArgumentException("Owner ID required", nameof(ownerId));

            if (!Enum.IsDefined(ownerType))
                throw new ArgumentException("Owner type required", nameof(ownerType));

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = fileName.Trim(),
                Url = url.Trim(),
                DocumentType = documentType,
                DocumentNumber = documentNumber.Trim(),
                IssuedDate = issuedDate,
                ExpiryDate = expiryDate,
                AssociatedValue = associatedValue,
                OwnerId = ownerId,
                OwnerType = ownerType,
                Status = DocumentStatus.PendingVerification,
                Version = 1,
            };

            return document;
        }

        // Behavior: Rename (fixed - now mutates same object)
        public void Rename(string newFileName)
        {
            if (string.IsNullOrWhiteSpace(newFileName))
                throw new ArgumentException("File name cannot be empty.", nameof(newFileName));

            var oldFileName = FileName;
            FileName = newFileName.Trim();         
        }

        // Behavior: ChangeUrl (fixed - now mutates same object)
        public void ChangeUrl(string newUrl)
        {
            if (!Uri.TryCreate(newUrl, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid document URL.", nameof(newUrl));

            var oldUrl = Url;
            Url = newUrl.Trim();          
        }

        // Behavior: Verify (fixed parameter)
        public void Verify(string verifiedBy)
        {
            if (string.IsNullOrWhiteSpace(verifiedBy))
                throw new ArgumentException("Verified by required", nameof(verifiedBy));

            if (Status == DocumentStatus.Verified)
                throw new InvalidOperationException("Document already verified");

            if (ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot verify expired document");

            Status = DocumentStatus.Verified;
            VerifiedAt = DateTime.UtcNow;
        }

        // Behavior: Renew (fixed - now uses OwnerType)
        public Document Renew(
            string newDocumentNumber,
            DateTime newIssuedDate,
            DateTime newExpiryDate,
            string renewedBy)
        {
            if (Status != DocumentStatus.Verified && Status != DocumentStatus.Expired)
                throw new InvalidOperationException("Only verified or expired documents can be renewed");

            if (newExpiryDate <= newIssuedDate)
                throw new ArgumentException("Expiry must be after issue date");

            // Create new version
            var renewed = new Document
            {
                Id = Guid.NewGuid(),
                FileName = FileName,
                Url = Url,
                DocumentType = DocumentType,
                DocumentNumber = newDocumentNumber.Trim(),
                IssuedDate = newIssuedDate,
                ExpiryDate = newExpiryDate,
                AssociatedValue = AssociatedValue,
                OwnerId = OwnerId,
                OwnerType = OwnerType,
                Status = DocumentStatus.PendingVerification,
                Version = Version + 1,
                ReplacesDocumentId = Id,
            };

            // Mark old as replaced
            Status = DocumentStatus.Replaced;

            return renewed;
        }

        // Behavior: Auto-expire
        public void CheckAndExpire()
        {
            if (ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow &&
                Status != DocumentStatus.Expired && Status != DocumentStatus.Replaced)
            {
                Status = DocumentStatus.Expired;
            }
        }

        // Behavior: Reject
        public void Reject(string reason, string rejectedBy)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason required", nameof(reason));

            if (string.IsNullOrWhiteSpace(rejectedBy))
                throw new ArgumentException("Rejected by required", nameof(rejectedBy));

            if (Status != DocumentStatus.PendingVerification)
                throw new InvalidOperationException("Only pending documents can be rejected");

            Status = DocumentStatus.Rejected;
        }

        // Query methods
        public bool IsActive()
        {
            return Status == DocumentStatus.Verified &&
                   (!ExpiryDate.HasValue || ExpiryDate > DateTime.UtcNow);
        }

        public bool IsExpiringSoon(int daysThreshold = 30)
        {
            return ExpiryDate.HasValue &&
                   Status == DocumentStatus.Verified &&
                   ExpiryDate > DateTime.UtcNow &&
                   ExpiryDate <= DateTime.UtcNow.AddDays(daysThreshold);
        }

        // Helper to check ownership
        public bool IsOwnedBy(Guid ownerId, string ownerType)
        {
            return OwnerId == ownerId && Equals(ownerType, StringComparison.OrdinalIgnoreCase);
        }
    }
}
