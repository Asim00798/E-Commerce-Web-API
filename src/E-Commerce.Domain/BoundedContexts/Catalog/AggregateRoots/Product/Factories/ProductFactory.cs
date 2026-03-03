using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Catalog.ValueObjects;
using E_Commerce.Domain.SharedKernel.ValueObjects;

public static class ProductFactory
{
    public static Product CreateWithDefaults(string name)
    {
        // Example: initialize defaults
        Product product = new Product(
            new ProductDescription(name)
            ,new Money(400),Guid.Empty
            );
        // product.AddChild(...);
        // product.RegisterProduct(...);
        return product;
    }
}
