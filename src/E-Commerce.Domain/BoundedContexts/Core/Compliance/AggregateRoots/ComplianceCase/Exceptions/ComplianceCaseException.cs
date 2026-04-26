using System;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.Exceptions
{
    public class ComplianceCaseException : Compliance.Exceptions.ComplianceDomainException
    {
        public ComplianceCaseException() { }

        public ComplianceCaseException(string message) : base(message) { }

        public ComplianceCaseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
