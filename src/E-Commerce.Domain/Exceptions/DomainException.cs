using System;

namespace E_Commerce.Domain.Exceptions
{
    /// <summary>
    /// Base type for all domain-level exceptions.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException() { }

        protected DomainException(string message)
            : base(message) { }

        protected DomainException(string message, Exception inner)
            : base(message, inner) { }
    }
}
