using E_Commerce.Application.Shared.Persistence;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repositories
{
    public class ProcessedEventRepository : Repository<ProcessedEvent>,IProcessedEventRepository
    {
        public ProcessedEventRepository(AppDbContext dbContext):base(dbContext)
        {}

        public async Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProcessedEvents
                .AnyAsync(e => e.EventId == eventId, cancellationToken);
        }

        public async Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var processedEvent = new ProcessedEvent
            {
                EventId = eventId,
                ProcessedAt = DateTime.UtcNow
            };
            _dbContext.ProcessedEvents.Add(processedEvent);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
