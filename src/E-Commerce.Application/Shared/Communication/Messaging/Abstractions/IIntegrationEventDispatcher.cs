namespace E_Commerce.Application.Shared.Communication.Messaging.Abstractions
{
    public interface IIntegrationEventDispatcher
    {
        Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
    }
}
