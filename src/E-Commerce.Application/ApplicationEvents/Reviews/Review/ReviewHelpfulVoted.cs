using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Review
{
    public sealed class ReviewHelpfulVoted : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewHelpfulVoted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}