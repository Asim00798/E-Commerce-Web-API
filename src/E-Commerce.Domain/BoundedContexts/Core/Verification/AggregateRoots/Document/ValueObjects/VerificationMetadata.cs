using E_Commerce.Domain.BoundedContexts.Core.Verification.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects
{
    /// <summary>
    /// Contains supplementary data related to a verifier's action, such as confidence scores or provider reference IDs.
    /// This is an immutable value object used in <see cref="VerificationRecord"/>.
    /// </summary>
    public sealed record VerificationMetadata
    {
        public decimal? ConfidenceScore { get; }
        public Guid? ProviderReference { get; }
        public string? Remarks { get; }

        private VerificationMetadata(decimal? confidenceScore, Guid? providerReference, string? remarks)
        {
            ConfidenceScore = confidenceScore;
            ProviderReference = providerReference;
            Remarks = remarks;
        }

        /// <summary>
        /// Creates metadata with a confidence score (e.g., from an automated check).
        /// </summary>
        public static VerificationMetadata FromConfidence(decimal confidenceScore, string? remarks = null)
        {
            ValidateConfidenceScore(confidenceScore);
            return new VerificationMetadata(confidenceScore, null, remarks?.Trim());
        }

        /// <summary>
        /// Creates metadata referencing an external verification provider.
        /// </summary>
        public static VerificationMetadata FromExternalProvider(Guid providerReference, string? remarks = null)
        {
            ValidateProviderReference(providerReference);
            return new VerificationMetadata(null, providerReference, remarks?.Trim());
        }

        /// <summary>
        /// Creates metadata with manual remarks (e.g., admin notes).
        /// </summary>
        public static VerificationMetadata WithRemarks(string remarks)
        {
            ValidateRemarks(remarks);
            return new VerificationMetadata(null, null, remarks.Trim());
        }

        /// <summary>
        /// Returns an empty metadata instance when no additional information is needed.
        /// </summary>
        public static VerificationMetadata Empty() => new(null, null, null);

        private static void ValidateConfidenceScore(decimal confidenceScore)
        {
            if (confidenceScore is < 0 or > 100)
                throw new VerificationDomainException("Confidence score must be between 0 and 100.");
        }

        private static void ValidateProviderReference(Guid providerReference)
        {
            if (providerReference == Guid.Empty)
                throw new VerificationDomainException("Provider reference cannot be empty.");
        }

        private static void ValidateRemarks(string? remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
                throw new VerificationDomainException("Remarks are required.");
            if (remarks.Length > 500)
                throw new VerificationDomainException("Remarks cannot exceed 500 characters.");
            if (remarks.Any(char.IsControl))
                throw new VerificationDomainException("Remarks cannot contain control characters.");
        }
    }
}

