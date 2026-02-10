using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Rating
{
    public sealed class RatingFlagged : DomainEvent
    {
        public Guid AggregateId { get; }

        public RatingFlagged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}