
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.Exceptions
{
    public class VerificationDomainException : DomainException
    {
        public VerificationDomainException(string message) : base(message)
        {
        }
        public VerificationDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
