using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

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
            .AnyAsync(e => e.EventId == eventId && e.HandlerIdentifier == handlerIdentifier, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid eventId,
        string handlerIdentifier,
        CancellationToken cancellationToken)
    {
        // Use an upsert-safe approach to avoid duplicate-key exceptions on retries.
        // We check first; this is safe because it's called after the handler succeeds,
        // and there's no concurrent write for the same (EventId, HandlerId).
        if (!await IsProcessedAsync(eventId, handlerIdentifier, cancellationToken))
        {
            var processedEvent = new ProcessedEvent
            {
                EventId = eventId,
                HandlerIdentifier = handlerIdentifier,
                ProcessedAt = DateTime.UtcNow
            };
            await _dbContext.Set<ProcessedEvent>().AddAsync(processedEvent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}