using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Serialization;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Implementation;

public class OutboxMessageWriter : IOutboxMessageWriter
{
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly OutboxSerializer _serializer;

    public OutboxMessageWriter(
        IOutboxMessageRepository outboxRepository,
        OutboxSerializer serializer)
    {
        _outboxRepository = outboxRepository;
        _serializer = serializer;
    }

    public async Task WriteAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var eventType = integrationEvent.GetType();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            // Assembly-qualified name so Type.GetType can resolve it at dispatch
            // time. Version and culture are omitted so future application version
            // bumps do not orphan existing rows.
            EventType = $"{eventType.FullName}, {eventType.Assembly.GetName().Name}",
            Payload = _serializer.Serialize(integrationEvent),
            OccurredAt = integrationEvent.OccurredAt,
            Status = OutboxMessageStatus.Pending
        };

        await _outboxRepository.AddAsync(message, cancellationToken);
    }
}