namespace E_Commerce.Application.Shared.Persistence
{
    public interface IProcessedEventRepository
    {
        Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken);
        Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken);
    }
}
