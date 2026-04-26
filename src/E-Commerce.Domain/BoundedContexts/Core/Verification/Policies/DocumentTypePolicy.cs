using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.Policies
{
    /// <summary>
    /// Pure domain policy determining if a document type is valid and allowed within the system.
    /// This removes hardcoded logic from the DocumentType value object itself.
    /// </summary>
    public class DocumentTypePolicy
    {
        private static readonly HashSet<string> _allowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Passport", "NationalId", "DrivingLicense", "TaxCertificate",
            "BusinessLicense", "Invoice", "BankStatement", "ProofOfAddress",
            "InsuranceCertificate", "ContractAgreement"
        };

        public virtual bool IsAllowed(DocumentType documentType)
        {
            if (documentType == null || string.IsNullOrWhiteSpace(documentType.Value))
                return false;

            return _allowedTypes.Contains(documentType.Value);
        }
        
        public virtual IReadOnlySet<string> GetAllowedTypes() => _allowedTypes;
    }
}
