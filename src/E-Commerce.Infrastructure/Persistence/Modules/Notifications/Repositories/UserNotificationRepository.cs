using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Repositories;

public class UserNotificationRepository : IUserNotificationRepository
{
    private readonly AppDbContext _db;

    public UserNotificationRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(UserNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(dto);
        _db.Set<UserNotification>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserNotificationDto>> GetByUserIdAsync(
        Guid userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var entities = await _db.Set<UserNotification>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    public async Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Set<UserNotification>()
            .CountAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Set<UserNotification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task<UserNotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<UserNotification>().FindAsync(new object[] { id }, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<UserNotification>().FindAsync(new object[] { id }, cancellationToken);
        if (entity is not null)
        {
            entity.IsRead = true;
            entity.ReadAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static UserNotification MapToEntity(UserNotificationDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        Type = dto.Type,
        Title = dto.Title,
        Message = dto.Message,
        PayloadJson = dto.PayloadJson,
        SourceEventId = dto.SourceEventId,
        CreatedAtUtc = dto.CreatedAtUtc,
        ReadAtUtc = dto.ReadAtUtc,
        IsRead = dto.IsRead
    };

    private static UserNotificationDto MapToDto(UserNotification entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        Type = entity.Type,
        Title = entity.Title,
        Message = entity.Message,
        PayloadJson = entity.PayloadJson,
        SourceEventId = entity.SourceEventId,
        CreatedAtUtc = entity.CreatedAtUtc,
        ReadAtUtc = entity.ReadAtUtc,
        IsRead = entity.IsRead
    };
}