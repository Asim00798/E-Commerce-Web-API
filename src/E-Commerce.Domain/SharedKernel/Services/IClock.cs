namespace E_Commerce.Domain.SharedKernel.Services;

/// <summary>
/// Abstraction over system time to support deterministic testing
/// and avoid direct dependencies on <see cref="DateTime.UtcNow"/>.
/// </summary>
public interface IClock
{
    /// <summary>
    /// The current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}