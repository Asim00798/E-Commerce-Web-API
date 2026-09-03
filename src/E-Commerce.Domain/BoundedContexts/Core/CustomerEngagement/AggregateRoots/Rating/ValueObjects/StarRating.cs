using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;

/// <summary>
/// Represents a star rating on a 1-to-5 scale.
/// Immutable value object owned by the Rating aggregate.
/// </summary>
public sealed record StarRating
{
    public int Value { get; }

    public StarRating(int value)
    {
        if (value < 1 || value > 5)
            throw new BusinessRuleViolationException("Star rating must be between 1 and 5.");

        Value = value;
    }

    public static StarRating From(int value) => new(value);
}