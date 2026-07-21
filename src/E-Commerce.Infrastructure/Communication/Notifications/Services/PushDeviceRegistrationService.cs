using E_Commerce.Application.Modules.Notifications.Models;
using E_Commerce.Application.Shared.Communication.Notifications.Services;
using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;

namespace E_Commerce.Infrastructure.Communication.Notifications.Services;

internal sealed class PushDeviceRegistrationService : IPushDeviceRegistrationService
{
    private readonly IPushDeviceRepository _deviceRepo;

    public PushDeviceRegistrationService(IPushDeviceRepository deviceRepo) => _deviceRepo = deviceRepo;

    public async Task<Guid> RegisterAsync(RegisterPushDeviceModel model, CancellationToken cancellationToken)
    {
        var entity = new PushDevice
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            FirebaseInstallationId = model.FirebaseInstallationId,
            Platform = Enum.TryParse<PushDevicePlatfrom>(model.Platform, out var platform)
                ? platform
                : PushDevicePlatfrom.None,
            IsActive = true
        };

        await _deviceRepo.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task DeactivateAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        await _deviceRepo.DeactivateAsync(deviceId, cancellationToken);
    }
}