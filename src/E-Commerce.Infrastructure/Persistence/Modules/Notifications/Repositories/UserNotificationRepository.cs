using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Notifications.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserNotificationRepository"/>.
/// Maps between the <see cref="UserNotificationDto"/> (Application layer)
/// and the <see cref="UserNotification"/> entity (Persistence layer).
/// </summary>
internal sealed class UserNotificationRepository
    : IUserNotificationRepository
{
    private readonly AppDbContext _db;

    public UserNotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task AddAsync(
        UserNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            PayloadJson = dto.PayloadJson,
            SourceEventId = dto.SourceEventId,
            CreatedAtUtc = dto.CreatedAtUtc,
            IsRead = false
        };

        await _db.Set<UserNotification>()
            .AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserNotificationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<UserNotification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity == null ? null : MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserNotificationDto>> GetByUserIdAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _db.Set<UserNotification>()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return notifications.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Set<UserNotification>()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Set<UserNotification>()
            .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Set<UserNotification>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null || entity.IsRead)
            return;

        entity.IsRead = true;
        entity.ReadAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Maps a persistence entity to the Application‑layer DTO.
    /// </summary>
    private static UserNotificationDto MapToDto(UserNotification entity)
    {
        return new UserNotificationDto
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
}