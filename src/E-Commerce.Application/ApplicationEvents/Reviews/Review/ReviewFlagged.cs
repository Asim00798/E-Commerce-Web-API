using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Review
{
    public sealed class ReviewFlagged : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewFlagged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}