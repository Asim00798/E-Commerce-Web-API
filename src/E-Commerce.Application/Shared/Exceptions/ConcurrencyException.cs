namespace E_Commerce.Application.Shared.Exceptions;

/// <summary>
/// Thrown when a database operation fails due to an optimistic concurrency conflict.
/// Infrastructure catches provider-specific concurrency exceptions (e.g., EF Core
/// DbUpdateConcurrencyException) and rethrows this neutral exception so higher
/// layers do not depend on persistence technology.
/// </summary>
public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}