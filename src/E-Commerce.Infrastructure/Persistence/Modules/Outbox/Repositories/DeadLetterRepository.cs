using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Outbox.Repository
{
    public class DeadLetterRepository : IDeadLetterRepository
    {
        private readonly AppDbContext _dbContext;

        public DeadLetterRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(DeadLetterMessage message, CancellationToken cancellationToken)
        {
            await _dbContext.Set<DeadLetterMessage>().AddAsync(message, cancellationToken);
        }

        public async Task<List<DeadLetterMessage>> GetDeadLetteredAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Set<DeadLetterMessage>()
                .Where(d => d.Status == DeadLetterStatus.DeadLettered)
                .OrderBy(d => d.DeadLetteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsReprocessingAsync(Guid deadLetterId, CancellationToken cancellationToken)
        {
            var dead = await _dbContext.Set<DeadLetterMessage>().FindAsync(new object[] { deadLetterId }, cancellationToken);
            if (dead != null)
            {
                dead.Status = DeadLetterStatus.Reprocessing;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteAsync(Guid deadLetterId, CancellationToken cancellationToken)
        {
            var dead = await _dbContext.Set<DeadLetterMessage>().FindAsync(new object[] { deadLetterId }, cancellationToken);
            if (dead != null)
            {
                _dbContext.Set<DeadLetterMessage>().Remove(dead);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}