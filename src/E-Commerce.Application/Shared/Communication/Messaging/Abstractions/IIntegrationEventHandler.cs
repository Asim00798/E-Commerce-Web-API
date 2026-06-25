namespace E_Commerce.Application.Shared.Communication.Messaging.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
    {
        Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken);
    }
}


