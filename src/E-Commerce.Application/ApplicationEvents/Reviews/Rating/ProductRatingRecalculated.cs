using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Rating
{
    public sealed class ProductRatingRecalculated : DomainEvent
    {
        public Guid AggregateId { get; }

        public ProductRatingRecalculated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}