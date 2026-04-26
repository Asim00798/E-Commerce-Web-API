using System;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Exceptions
{
    public class ComplianceDomainException : DomainException
    {
        public ComplianceDomainException()
        {
        }

        public ComplianceDomainException(string message)
            : base(message)
        {
        }

        public ComplianceDomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
