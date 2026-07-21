using E_Commerce.Application.Modules.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Services;

/// <summary>
/// Service contract for registering and deregistering push devices.
/// The Application layer works only with its own DTOs.
/// </summary>
public interface IPushDeviceRegistrationService
{
    Task<Guid> RegisterAsync(RegisterPushDeviceModel model, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid deviceId, CancellationToken cancellationToken = default);
}