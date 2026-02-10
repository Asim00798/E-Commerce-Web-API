using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Review
{
    public sealed class ReviewReported : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewReported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}