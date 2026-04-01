using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Exceptions
{

    /// <summary>
    /// Exceptions related to
    /// product operations within the catalog context.
    /// </summary>
    public sealed class ProductException : DomainException
    {
        public string Rule { get; }
        public ProductException(string rule)
            : base($"Business rule violated: {rule}")
        {
            Rule = rule;
        }
        public ProductException(string rule, string details)
            : base($"Business rule violated: {rule}. {details}")
        {
            Rule = rule;
        }
    }
}
