using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Review
{
    public sealed class ReviewEdited : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewEdited(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}