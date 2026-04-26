namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects
{
    /// <summary>
    /// Represents the classification of a document (e.g., Passport, Invoice, License).
    /// Enforces naming constraints to ensure consistency across the verification pipeline.
    /// Validation is handled by policies like DocumentTypePolicy.
    /// </summary>
    public sealed record DocumentType
    {
        public string Value { get; }

        public DocumentType(string value)
        {
            EnsureNotEmpty(value);
            Value = value.Trim();
        }

        private static void EnsureNotEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Document type must not be empty.", nameof(value));
        }

        public override string ToString() => Value;
    }
}
