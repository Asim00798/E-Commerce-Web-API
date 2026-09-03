namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Enums;

// <summary>
// Represents the status of a product.
// Draft: The product exists in Catalog but isn't available to customers yet.
// Published: The product is officially available in the catalog for customers to see/select.
// Discontinued: The product has been permanently or indefinitely removed from active selling.
// </summary>
public enum ProductStatus
{
    Draft = 1,
    Published = 2,
    Discontinued = 3
}