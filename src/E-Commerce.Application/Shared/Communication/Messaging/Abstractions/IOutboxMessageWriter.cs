namespace E_Commerce.Application.Shared.Communication.Messaging.Abstractions
{
    public interface IOutboxMessageWriter
    {
        Task WriteAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
