using E_Commerce.Infrastructure.Communication.Notifications.Entities;

namespace E_Commerce.Infrastructure.Communication.Notifications.Contracts;

/// <summary>
/// Persistence abstraction for push device registrations.
/// This interface lives in Infrastructure because it is only consumed by Infrastructure components.
/// </summary>
public interface IPushDeviceRepository
{
    Task<List<PushDevice>> GetActiveDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(PushDevice device, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task DeactivateByFidAsync(string firebaseInstallationId, CancellationToken cancellationToken = default);
}