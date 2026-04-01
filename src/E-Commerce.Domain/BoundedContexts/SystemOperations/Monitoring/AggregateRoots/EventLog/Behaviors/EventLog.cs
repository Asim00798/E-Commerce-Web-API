using E_Commerce.Domain.SharedKernel.Events.Abstractions;
using System.Text.Json;
/*
1- EventName
   Readable value for debugging/logging:
       ProductPublished
       ProductPriceChanged
       BrandCreated
   Much easier to inspect in DB than full type names.

2?- EventType uses FullName
   EventType = @event.GetType().FullName!;
   This prevents problems later if two events have the same name in different contexts.

3?- ProcessedOnUtc
   Now you can track:
       - when event was created
       - when it was handled
   This is extremely useful in real production debugging.

4?- Version
   Not event sourcing — just future-safe:
       ProductPublished v1
       ProductPublished v2
       Support evolving event structures.

Final result:
       EventLog now supports:
       - auditing
       - debugging
       - replay if needed
       - production monitoring
       - future event evolution
   without turning project into event sourcing.
*/
namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Monitoring.AggregateRoots.EventLog.Behaviors
{
    public class EventLog
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        // Where the event happened
        public Guid AggregateId { get; private set; }
        public string AggregateType { get; private set; } = null!;

        // What happened
        public string EventType { get; private set; } = null!;
        public string EventName { get; private set; } = null!;

        // Serialized event payload
        public string Payload { get; private set; } = null!;

        // When it happened
        public DateTime OccurredOnUtc { get; private set; }

        // Processing / reliability
        public bool Processed { get; private set; } = false;
        public DateTime? ProcessedOnUtc { get; private set; }

        // Future-safe (not event sourcing, just versioning support)
        public int Version { get; private set; } = 1;

        private EventLog() { } // EF Core constructor

        public EventLog(Guid aggregateId, string aggregateType, IDomainEvent @event)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;

            EventType = @event.GetType().FullName!;
            EventName = @event.GetType().Name;

            Payload = JsonSerializer.Serialize(@event);

            OccurredOnUtc = DateTime.UtcNow;
        }

        public void MarkProcessed()
        {
            Processed = true;
            ProcessedOnUtc = DateTime.UtcNow;
        }
    }
}