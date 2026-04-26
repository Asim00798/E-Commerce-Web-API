namespace E_Commerce.Infrastructure.Common.Services;

/// <summary>
/// Infrastructure implementation of IDateTime / clock abstraction.
/// Returns UTC time, allowing easy substitution in tests.
/// </summary>
public sealed class ClockService
{
    /// <summary>Returns the current UTC date and time.</summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    // TODO: Implement IDateTime interface from Application layer
}
