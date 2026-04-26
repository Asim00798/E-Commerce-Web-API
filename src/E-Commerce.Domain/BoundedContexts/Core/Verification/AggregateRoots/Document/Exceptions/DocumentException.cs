using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Exceptions
{
    /// <summary>
    /// Represents a domain-level violation raised within the Verification bounded context.
    /// Thrown when a business invariant or domain rule is broken.
    /// </summary>
    public class DocumentException : DomainException
    {
        public DocumentException(string message) : base(message) { }

        public DocumentException(string message, Exception inner) : base(message, inner) { }
    }
}
