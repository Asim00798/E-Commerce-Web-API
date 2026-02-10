using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Rating
{
    public sealed class RatingWithdrawn : DomainEvent
    {
        public Guid RatingWithdrawnId { get; }

        public RatingWithdrawn(Guid ratingWithdrawnId)
        {
            RatingWithdrawnId = ratingWithdrawnId;
        }
    }
}