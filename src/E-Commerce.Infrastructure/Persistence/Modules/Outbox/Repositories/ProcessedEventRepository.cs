using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repositories;

public class ProcessedEventRepository : Repository<ProcessedEvent>, IProcessedEventRepository
{
    public ProcessedEventRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<bool> IsProcessedAsync(
        Guid eventId,
        string handlerIdentifier,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<ProcessedEvent>()
            .AnyAsync(
                e => e.EventId == eventId && e.HandlerIdentifier == handlerIdentifier,
                cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid eventId,
        string handlerIdentifier,
        CancellationToken cancellationToken)
    {
        // Fast path: already recorded by a previous attempt or a concurrent worker.
        if (await IsProcessedAsync(eventId, handlerIdentifier, cancellationToken))
            return;

        var processedEvent = new ProcessedEvent
        {
            EventId = eventId,
            HandlerIdentifier = handlerIdentifier,
            ProcessedAt = DateTime.UtcNow
        };

        try
        {
            await _dbContext.Set<ProcessedEvent>().AddAsync(processedEvent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // A concurrent worker inserted the same (EventId, HandlerIdentifier) row
            // between our check and our insert. The composite PK rejected the duplicate,
            // which means the handler's side effect has already been recorded.
            //
            // Detach the failed entity so subsequent operations on the same DbContext
            // don't keep retrying to insert it.
            _dbContext.Entry(processedEvent).State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Detects a SQL Server unique-key or primary-key constraint violation.
    /// Error 2601 = duplicate key row in object with unique index.
    /// Error 2627 = violation of PRIMARY KEY or UNIQUE constraint.
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql
            && (sql.Number == 2601 || sql.Number == 2627);
    }
}