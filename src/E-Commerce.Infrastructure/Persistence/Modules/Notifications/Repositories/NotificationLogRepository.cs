using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<NotificationLog>().Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
