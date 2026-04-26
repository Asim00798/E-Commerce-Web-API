using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects
{
    /// <summary>
    /// Represents a historical immutable record of a verification attempt on a document.
    /// Owned by the Document Aggregate.
    /// </summary>
    public sealed record VerificationRecord
    {
        public Guid VerifierId { get; private set; }
        public VerificationMethod Method { get; private set; }
        public VerificationStatus ResultStatus { get; private set; }
        public VerificationMetadata? Metadata { get; private set; }
        public string? Reason { get; private set; }

        internal VerificationRecord(Guid verifierId, VerificationMethod method, VerificationStatus resultStatus, VerificationMetadata? metadata, string? reason = null)
        {
            ValidateVerifierId(verifierId);
            ValidateRejectionReason(resultStatus, reason);

            VerifierId = verifierId;
            Method = method;
            ResultStatus = resultStatus;
            Metadata = metadata ?? VerificationMetadata.Empty();
            Reason = reason;
        }

        private static void ValidateVerifierId(Guid verifierId)
        {
            if (verifierId == Guid.Empty)
                throw new ArgumentException("VerifierId cannot be empty.", nameof(verifierId));
        }

        private static void ValidateRejectionReason(VerificationStatus resultStatus, string? reason)
        {
            if (resultStatus == VerificationStatus.Rejected && string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A rejection reason is required when the result status is Rejected.", nameof(reason));
            }
        }
    }
}
