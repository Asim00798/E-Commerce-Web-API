using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts
{
    public interface IDeadLetterRepository
    {
        Task AddAsync(DeadLetterMessage message, CancellationToken cancellationToken);
        Task<List<DeadLetterMessage>> GetDeadLetteredAsync(CancellationToken cancellationToken);
        Task MarkAsReprocessingAsync(Guid deadLetterId, CancellationToken cancellationToken);
        Task DeleteAsync(Guid deadLetterId, CancellationToken cancellationToken);
    }
}