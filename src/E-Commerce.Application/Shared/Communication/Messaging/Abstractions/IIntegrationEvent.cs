namespace E_Commerce.Application.Shared.Communication.Messaging.Abstractions
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
    }
}


