namespace E_Commerce.Application.Shared.Communication.Messaging.Abstractions
{
    public interface IProcessedEventRepository
    {
        /// <summary>
        /// Checks whether a specific handler has already processed the given event.
        /// </summary>
        Task<bool> IsProcessedAsync(Guid eventId, string handlerIdentifier, CancellationToken cancellationToken);

        /// <summary>
        /// Marks a specific handler as having successfully processed the event.
        /// </summary>
        Task MarkAsProcessedAsync(Guid eventId, string handlerIdentifier, CancellationToken cancellationToken);
    }
}
