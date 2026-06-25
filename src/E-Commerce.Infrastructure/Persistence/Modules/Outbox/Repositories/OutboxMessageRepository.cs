using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;

namespace E_Commerce.Infrastructure.Persistence.Outbox.Repository;

public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending ||
                        m.Status == OutboxMessageStatus.Failed)
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var msg = await _dbContext.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (msg != null)
        {
            msg.Status = OutboxMessageStatus.Processed;
            msg.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken)
    {
        var msg = await _dbContext.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (msg != null)
        {
            msg.Status = OutboxMessageStatus.Failed;
            msg.Error = error;
            msg.RetryCount++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsDeadLetteredAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var msg = await _dbContext.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (msg != null)
        {
            msg.Status = OutboxMessageStatus.DeadLettered;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}