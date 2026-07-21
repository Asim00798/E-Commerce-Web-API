using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Repositories;

public class NotificationPreferencesRepository
    : Repository<NotificationPreferences>, INotificationPreferencesRepository
{
    public NotificationPreferencesRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<NotificationPreferences?> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}