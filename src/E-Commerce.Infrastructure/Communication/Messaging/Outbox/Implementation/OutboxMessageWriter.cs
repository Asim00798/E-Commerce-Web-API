using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Serialization;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Implementation;

public class OutboxMessageWriter : IOutboxMessageWriter
{
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly OutboxSerializer _serializer;

    public OutboxMessageWriter(IOutboxMessageRepository outboxRepository, OutboxSerializer serializer)
    {
        _outboxRepository = outboxRepository;
        _serializer = serializer;
    }

    public async Task WriteAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = integrationEvent.GetType().FullName!,
            Payload = _serializer.Serialize(integrationEvent),
            OccurredAt = integrationEvent.OccurredAt,
            Status = OutboxMessageStatus.Pending
        };

        await _outboxRepository.AddAsync(message, cancellationToken);
    }
}
