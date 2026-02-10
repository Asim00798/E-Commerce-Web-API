using E_Commerce.Domain.DomainEvents.Abstractions;

namespace E_Commerce.Domain.DomainEvents
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    }
}
