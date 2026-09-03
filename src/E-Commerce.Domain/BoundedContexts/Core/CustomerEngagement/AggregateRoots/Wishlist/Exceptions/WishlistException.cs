using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Wishlist.Exceptions;

public sealed class WishlistException : DomainException
{
    public WishlistException(string message) : base(message)
    {}

    public WishlistException(string message, Exception innerException) : base(message, innerException)
    {}
}