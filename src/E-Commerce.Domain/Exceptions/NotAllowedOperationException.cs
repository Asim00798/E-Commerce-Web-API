namespace E_Commerce.Domain.Exceptions
{
    /// <summary>
    /// Thrown when an operation is not allowed
    /// in the current domain state.
    /// </summary>
    public sealed class NotAllowedOperationException : DomainException
    {
        public NotAllowedOperationException(string operation)
            : base($"Operation not allowed: {operation}") { }

        public NotAllowedOperationException(string operation, string reason)
            : base($"Operation not allowed: {operation}. {reason}") { }
    }
}
