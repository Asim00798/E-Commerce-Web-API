using E_Commerce.Domain.SharedKernel.Services;

namespace E_Commerce.Infrastructure.Time;

/// <summary>
/// Default implementation of <see cref="IClock"/> that delegates
/// to the real system clock.
/// </summary>
public sealed class ClockService : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}