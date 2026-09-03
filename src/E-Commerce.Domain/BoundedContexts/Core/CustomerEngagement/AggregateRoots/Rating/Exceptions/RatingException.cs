using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Exceptions;

/// <summary>
/// Represents a domain error within the Rating aggregate.
/// </summary>
public sealed class RatingException : DomainException
{
    public RatingException(string message) : base(message)
    {}

    public RatingException(string message, Exception innerException) : base(message, innerException)
    {}
}