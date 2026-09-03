namespace E_Commerce.Application.BoundedContexts.Onboarding.Abstractions;

/// <summary>
/// Performs housekeeping operations on registrations, such as deleting
/// expired records. This is a maintenance concern, not part of the
/// aggregate repository.
/// </summary>
public interface IRegistrationCleanupService
{
    /// <summary>
    /// Deletes all registrations whose expiration timestamp is earlier than
    /// <paramref name="utcNow"/>. Returns the number of removed records.
    /// </summary>
    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}