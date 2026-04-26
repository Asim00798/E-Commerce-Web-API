using E_Commerce.Domain.BoundedContexts.Core.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Exceptions
{
    /// <summary>
    /// Exception related to the Brand aggregate root.
    /// </summary>
    public sealed class BrandException : DomainException
    {
        public string Rule { get; }
        public BrandException(string rule)
            : base($"Business rule violated: {rule}")
        {
            Rule = rule;
        }
        public BrandException(string rule, string details)
            : base($"Business rule violated: {rule}. {details}")
        {
            Rule = rule;
        }
    }
}
