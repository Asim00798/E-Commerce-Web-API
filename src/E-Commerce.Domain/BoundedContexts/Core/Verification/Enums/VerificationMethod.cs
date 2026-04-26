namespace E_Commerce.Domain.BoundedContexts.Core.Verification.Enums
{
    /// <summary>
    /// Describes the mechanism used to verify a document.
    /// </summary>
    public enum VerificationMethod
    {
        /// <summary>A human reviewer manually inspects and verifies the document.</summary>
        Manual = 0,

        /// <summary>An automated system performs rule-based verification without human intervention.</summary>
        Auto = 1,

        /// <summary>Verification is delegated to a trusted third-party external provider.</summary>
        External = 2
    }
}
