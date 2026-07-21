using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Repositories;

public class PushDeviceRepository : IPushDeviceRepository
{
    private readonly AppDbContext _db;

    public PushDeviceRepository(AppDbContext db) => _db = db;

    public async Task<List<PushDevice>> GetActiveDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Set<PushDevice>()
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PushDevice device, CancellationToken cancellationToken = default)
    {
        _db.Set<PushDevice>().Add(device);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _db.Set<PushDevice>().FindAsync(new object[] { deviceId }, cancellationToken);
        if (device is not null)
        {
            device.IsActive = false;
            device.DeactivatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateByFidAsync(string firebaseInstallationId, CancellationToken cancellationToken = default)
    {
        var devices = await _db.Set<PushDevice>()
            .Where(d => d.FirebaseInstallationId == firebaseInstallationId && d.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var device in devices)
        {
            device.IsActive = false;
            device.DeactivatedAt = DateTime.UtcNow;
        }

        if (devices.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);
    }
}